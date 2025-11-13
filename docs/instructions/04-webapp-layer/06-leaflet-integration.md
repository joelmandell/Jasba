# 04-WebApp-Layer: Leaflet.js Integration

## Objective
Integrate Leaflet.js for interactive floor plan visualization and object placement.

## Prerequisites
- Completed: 04-webapp-layer/05-tenant-floorplan-management.md
- Basic understanding of JavaScript interop in Blazor

## Instructions

### 1. Install Leaflet.js

**File**: `src/SBAPro.WebApp/wwwroot/index.html`

Add in `<head>`:
```html
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
```

### 2. Create JavaScript Helper

**File**: `src/SBAPro.WebApp/wwwroot/js/leaflet-helper.js`

```javascript
window.leafletHelper = {
    map: null,
    
    initMap: function(elementId, imageUrl, width, height) {
        const bounds = [[0, 0], [height, width]];
        
        this.map = L.map(elementId, {
            crs: L.CRS.Simple,
            minZoom: -2,
            maxZoom: 2
        });
        
        L.imageOverlay(imageUrl, bounds).addTo(this.map);
        this.map.fitBounds(bounds);
        
        return true;
    },
    
    addMarker: function(x, y, icon, description) {
        if (!this.map) return;
        
        const marker = L.marker([y, x], {
            icon: L.divIcon({
                html: icon,
                className: 'inspection-object-icon'
            })
        }).addTo(this.map);
        
        marker.bindPopup(description);
        
        return true;
    }
};
```

### 3. Use in Blazor Component

**File**: `src/SBAPro.WebApp/Components/Pages/Tenant/FloorPlanEditor.razor`

```razor
@page "/tenant/floorplan-editor/{FloorPlanId:guid}"
@inject IJSRuntime JS

<div id="map" style="height: 600px;"></div>

@code {
    [Parameter]
    public Guid FloorPlanId { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("leafletHelper.initMap", 
                "map", 
                "/uploads/floorplan.png", 
                30, 
                20);
        }
    }
}
```

## Validation

1. Navigate to floor plan editor
2. Verify floor plan displays as map
3. Test zooming and panning
4. Verify coordinate system works

## Success Criteria

✅ Leaflet.js loads correctly  
✅ Floor plan displays as map  
✅ Coordinate system configured  
✅ Can zoom and pan  
