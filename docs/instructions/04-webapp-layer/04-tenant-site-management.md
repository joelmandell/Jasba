# 04-WebApp-Layer: TenantAdmin Site Management

## Objective
Implement the TenantAdmin interface for managing sites within their tenant.

## Prerequisites
- Completed: 04-webapp-layer/03-admin-tenant-management.md

## Instructions

**File**: `src/SBAPro.WebApp/Components/Pages/Tenant/Sites.razor`

The Sites page allows TenantAdmin to:
- View all sites for their tenant
- Create new sites
- Edit site details
- Delete sites

Key features:
- `@attribute [Authorize(Roles = "TenantAdmin")]`
- Automatic tenant filtering via global query filters
- CRUD operations for sites

## Validation

1. Login as demo@democompany.se
2. Navigate to /Tenant/Sites
3. Create a new site
4. Verify only sites for current tenant appear
5. Login as different tenant - should not see other tenant's sites

## Success Criteria

✅ TenantAdmin can view their sites  
✅ Can create new sites  
✅ Can edit/delete sites  
✅ Cannot see other tenant's sites  
✅ TenantId automatically set on new sites  
