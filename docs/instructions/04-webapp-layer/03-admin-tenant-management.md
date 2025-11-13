# 04-WebApp-Layer: SystemAdmin Tenant Management

## Objective
Implement the SystemAdmin interface for creating and managing tenants.

## Prerequisites
- Completed: 04-webapp-layer/02-authentication-pages.md

## Instructions

**File**: `src/SBAPro.WebApp/Components/Pages/Admin/Tenants.razor`

The Tenants page allows SystemAdmin to:
- View all tenants
- Create new tenants
- Edit tenant details
- Create tenant admin users

See existing implementation in `src/SBAPro.WebApp/Components/Pages/Admin/Tenants.razor`

Key features:
- `@attribute [Authorize(Roles = "SystemAdmin")]`
- Lists all tenants (uses `IgnoreQueryFilters()`)
- Create tenant form
- Create tenant admin user for each tenant

## Validation

1. Login as admin@sbapro.com
2. Navigate to /Admin/Tenants
3. Create a new tenant
4. Verify tenant appears in list
5. Create tenant admin for the new tenant

## Success Criteria

✅ SystemAdmin can view all tenants  
✅ Can create new tenants  
✅ Can create tenant admins  
✅ Other roles cannot access this page  
