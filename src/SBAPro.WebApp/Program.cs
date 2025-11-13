using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SBAPro.Core.Entities;
using SBAPro.Core.Interfaces;
using SBAPro.Infrastructure.Data;
using SBAPro.Infrastructure.Services;
using SBAPro.WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure Database
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

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
