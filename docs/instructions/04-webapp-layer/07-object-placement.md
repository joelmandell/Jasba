# 04-WebApp-Layer: Object Placement Editor

## Objective
Implement interactive object placement on floor plans with drag-and-drop.

## Prerequisites
- Completed: 04-webapp-layer/06-leaflet-integration.md

## Instructions

The Floor Plan Editor allows TenantAdmin to:
- Click on floor plan to place objects
- Select object type from dropdown
- Drag existing objects to new positions
- Delete objects
- Save object positions

**File**: `src/SBAPro.WebApp/Components/Pages/Tenant/FloorPlanEditor.razor`

See existing implementation for:
- Object type selection
- Click-to-place functionality
- Position saving to database
- Object rendering with icons

## Validation

1. Login as TenantAdmin
2. Navigate to floor plan editor
3. Select an object type (e.g., "Brandsläckare")
4. Click on floor plan to place object
5. Verify object appears with icon
6. Save and reload - object persists

## Success Criteria

✅ Can select object types  
✅ Can place objects by clicking  
✅ Objects display with correct icons  
✅ Positions save to database  
✅ Objects persist after reload  
