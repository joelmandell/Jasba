# 04-WebApp-Layer: TenantAdmin Floor Plan Management

## Objective
Implement the TenantAdmin interface for uploading and managing floor plan images.

## Prerequisites
- Completed: 04-webapp-layer/04-tenant-site-management.md

## Instructions

**File**: `src/SBAPro.WebApp/Components/Pages/Tenant/FloorPlans.razor`

The Floor Plans page allows TenantAdmin to:
- View all floor plans for each site
- Upload new floor plan images
- Set floor plan dimensions
- Delete floor plans

Key features:
- File upload handling with `InputFile`
- Image storage (base64 in database or file system)
- Floor plan metadata (width, height in meters)

## Validation

1. Login as TenantAdmin
2. Navigate to /Tenant/FloorPlans
3. Select a site
4. Upload a floor plan image
5. Set dimensions (e.g., 30m x 20m)
6. Verify floor plan is saved

## Success Criteria

✅ Can upload floor plan images  
✅ Can set dimensions  
✅ Images display correctly  
✅ Can delete floor plans  
