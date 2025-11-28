using System.Globalization;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using SBAPro.Core.Entities;
using SBAPro.Core.Interfaces;
using SBAPro.Infrastructure.Data;
using SBAPro.Infrastructure.Services;
using SBAPro.WebApp.Components;
using SBAPro.WebApp.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure Localization  
builder.Services.AddLocalization();

// Configure supported cultures - Swedish as default
var supportedCultures = new[]
{
    new CultureInfo("sv"),
    new CultureInfo("en")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("sv");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;
});

// Configure EasyCaching In-Memory provider for second level cache
builder.Services.AddEasyCaching(options =>
{
    options.UseInMemory();
});

// Register IEFCacheServiceProvider as Singleton FIRST
builder.Services.AddSingleton<IEFCacheServiceProvider, EFCacheServiceProviderWrapper>();


// Configure Database with cache interceptor
builder.Services.AddDbContextFactory<ApplicationDbContext>((serviceProvider, options) =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

// Also add regular DbContext for Identity (it requires a scoped DbContext, not just a factory)
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Add authorization
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Register application services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
builder.Services.AddScoped<IReportGenerator, QuestPdfReportGenerator>();

// Register background services
builder.Services.AddHostedService<InspectionReminderService>();

var app = builder.Build();

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var contextFactory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        await using var context = await contextFactory.CreateDbContextAsync();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        await DbInitializer.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Use Request Localization
app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Validation endpoints for infrastructure testing (development only)
if (app.Environment.IsDevelopment())
{
    app.MapGet("/test-services", (
        ApplicationDbContext dbContext,
        ITenantService tenantService,
        IEmailService emailService,
        IReportGenerator reportGenerator) =>
    {
        return Results.Ok(new
        {
            DbContext = dbContext != null ? "✓" : "✗",
            TenantService = tenantService != null ? "✓" : "✗",
            EmailService = emailService != null ? "✓" : "✗",
            ReportGenerator = reportGenerator != null ? "✓" : "✗"
        });
    });

    app.MapGet("/test-tenant-service", (ITenantService tenantService, HttpContext httpContext) =>
    {
        var user = httpContext.User;
        var tenantId = tenantService.TryGetTenantId();
        
        return Results.Ok(new
        {
            IsAuthenticated = user.Identity?.IsAuthenticated,
            UserName = user.Identity?.Name,
            TenantId = tenantId,
            TenantIdClaim = user.FindFirst("TenantId")?.Value
        });
    });

    app.MapGet("/test-email", async (IEmailService emailService) =>
    {
        try
        {
            await emailService.SendEmailAsync(
                "test@example.com",
                "Test Email from SBA Pro",
                "<h1>Test Email</h1><p>This is a test email with <strong>HTML</strong> content.</p><p>Swedish characters: åäö ÅÄÖ</p>"
            );
            
            return Results.Ok("Email sent successfully. Check MailHog at http://localhost:8025");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Email failed: {ex.Message}");
        }
    });
}

app.Run();

// Simple wrapper for EasyCaching provider
public class EFCacheServiceProviderWrapper : IEFCacheServiceProvider
{
    private readonly EasyCaching.Core.IEasyCachingProvider _cachingProvider;
    
    public EFCacheServiceProviderWrapper(EasyCaching.Core.IEasyCachingProviderFactory easyCachingProviderFactory)
    {
        _cachingProvider = easyCachingProviderFactory.GetCachingProvider("DefaultInMemory");
    }
    
    public void ClearAllCachedEntries()
    {
        _cachingProvider.Flush();
    }
    
    public EFCachedData? GetValue(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
    {
        var value = _cachingProvider.Get<EFCachedData>(cacheKey.KeyHash);
        return value.HasValue ? value.Value : null;
    }
    
    public void InvalidateCacheDependencies(EFCacheKey cacheKey)
    {
        _cachingProvider.Remove(cacheKey.KeyHash);
    }
    
    public void InvalidateCacheDependencies(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
    {
        _cachingProvider.Remove(cacheKey.KeyHash);
    }
    
    public void InsertValue(EFCacheKey cacheKey, EFCachedData? value, EFCachePolicy cachePolicy)
    {
        if (value != null)
        {
            _cachingProvider.Set(
                cacheKey.KeyHash,
                value,
                TimeSpan.FromMinutes(cachePolicy.CacheTimeout?.TotalMinutes ?? 30)
            );
        }
    }
    
    public bool StoreMetadata(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
    {
        return true;
    }
}
