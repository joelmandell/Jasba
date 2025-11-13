# 04-WebApp-Layer: Authentication Pages

## Objective
Implement login, logout, and access denied pages for user authentication.

## Prerequisites
- Completed: 04-webapp-layer/01-identity-setup.md
- Understanding of Blazor forms
- Understanding of ASP.NET Core Identity

## Overview

Authentication pages provide:
1. Login page with credentials form
2. Logout functionality
3. Access denied page
4. Return URL handling
5. Error message display

## Instructions

### 1. Create Login Page

**File**: `src/SBAPro.WebApp/Components/Pages/Account/Login.razor`

```razor
@page "/Account/Login"
@using Microsoft.AspNetCore.Identity
@using SBAPro.Core.Entities
@inject SignInManager<ApplicationUser> SignInManager
@inject NavigationManager Navigation

<PageTitle>Login</PageTitle>

<div class="container mx-auto max-w-md mt-10">
    <div class="bg-white shadow-md rounded px-8 pt-6 pb-8 mb-4">
        <h2 class="text-2xl font-bold mb-6 text-center">SBA Pro - Login</h2>
        
        @if (!string.IsNullOrEmpty(errorMessage))
        {
            <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
                @errorMessage
            </div>
        }

        <EditForm Model="model" OnValidSubmit="HandleLogin">
            <DataAnnotationsValidator />
            
            <div class="mb-4">
                <label class="block text-gray-700 text-sm font-bold mb-2">
                    Email
                </label>
                <InputText @bind-Value="model.Email" 
                          class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700" 
                          placeholder="your@email.com" />
                <ValidationMessage For="() => model.Email" class="text-red-500 text-xs mt-1" />
            </div>

            <div class="mb-6">
                <label class="block text-gray-700 text-sm font-bold mb-2">
                    Password
                </label>
                <InputText type="password" @bind-Value="model.Password" 
                          class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700" 
                          placeholder="********" />
                <ValidationMessage For="() => model.Password" class="text-red-500 text-xs mt-1" />
            </div>

            <div class="mb-4">
                <label class="flex items-center">
                    <InputCheckbox @bind-Value="model.RememberMe" class="mr-2" />
                    <span class="text-sm text-gray-700">Remember me</span>
                </label>
            </div>

            <button type="submit" 
                    class="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded w-full">
                Login
            </button>
        </EditForm>

        <div class="mt-6 text-center text-sm">
            <p class="text-gray-600">Demo Credentials:</p>
            <p class="text-gray-600">SystemAdmin: admin@sbapro.com / Admin@123</p>
            <p class="text-gray-600">TenantAdmin: demo@democompany.se / Demo@123</p>
            <p class="text-gray-600">Inspector: inspector@democompany.se / Inspector@123</p>
        </div>
    </div>
</div>

@code {
    private LoginModel model = new();
    private string? errorMessage;

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    private async Task HandleLogin()
    {
        try
        {
            var result = await SignInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                Navigation.NavigateTo(ReturnUrl ?? "/", forceLoad: true);
            }
            else
            {
                errorMessage = "Invalid login attempt.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
    }

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}
```

### 2. Create Logout Handler

**File**: `src/SBAPro.WebApp/Components/Pages/Account/Logout.razor`

```razor
@page "/Account/Logout"
@using Microsoft.AspNetCore.Identity
@using SBAPro.Core.Entities
@inject SignInManager<ApplicationUser> SignInManager
@inject NavigationManager Navigation

<PageTitle>Logging out...</PageTitle>

<p>Logging out...</p>

@code {
    protected override async Task OnInitializedAsync()
    {
        await SignInManager.SignOutAsync();
        Navigation.NavigateTo("/", forceLoad: true);
    }
}
```

### 3. Create Access Denied Page

**File**: `src/SBAPro.WebApp/Components/Pages/Account/AccessDenied.razor`

```razor
@page "/Account/AccessDenied"

<PageTitle>Access Denied</PageTitle>

<div class="container mx-auto max-w-md mt-10">
    <div class="bg-white shadow-md rounded px-8 pt-6 pb-8 mb-4">
        <h2 class="text-2xl font-bold mb-6 text-red-600">Access Denied</h2>
        <p class="mb-4">You do not have permission to access this page.</p>
        <p class="mb-4 text-gray-600">
            This page requires a specific role or permissions that your account does not have.
        </p>
        <div class="mt-6">
            <a href="/" class="text-blue-500 hover:text-blue-700">Return to Home</a>
            <span class="mx-2">|</span>
            <a href="/Account/Logout" class="text-blue-500 hover:text-blue-700">Logout</a>
        </div>
    </div>
</div>
```

## Validation Steps

### 1. Test Login

1. Navigate to http://localhost:5000/Account/Login
2. Enter: admin@sbapro.com / Admin@123
3. Click Login

**Expected**: Redirects to home page, user is logged in

### 2. Test Invalid Login

1. Enter incorrect password
2. Click Login

**Expected**: Error message displays

### 3. Test Logout

1. While logged in, navigate to /Account/Logout

**Expected**: Logged out, redirected to home

### 4. Test Access Denied

1. Login as Inspector
2. Try to access /Admin/Tenants

**Expected**: Redirected to AccessDenied page

## Success Criteria

✅ Login page displays correctly  
✅ Valid credentials log in successfully  
✅ Invalid credentials show error  
✅ Remember me checkbox works  
✅ Logout signs user out  
✅ Access denied page shows for unauthorized access  
✅ Return URL works after login  

## Next Steps

After completing this module:
1. Proceed to **03-admin-tenant-management.md**
2. Test all three user roles can login
3. Verify authentication persists across page navigation

## Additional Resources

- [Blazor Forms](https://docs.microsoft.com/en-us/aspnet/core/blazor/forms-validation)
- [ASP.NET Core SignInManager](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.signinmanager-1)
