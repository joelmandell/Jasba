// Inspection Map Manager for SBA Pro
let maps = {};
let markers = {};

export function initializeInspectionMap(mapId, imageUrl, imageWidth, imageHeight) {
    try {
        // Clean up existing map if any
        if (maps[mapId]) {
            maps[mapId].remove();
        }

        // Create map with simple coordinate system
        const map = L.map(mapId, {
            crs: L.CRS.Simple,
            minZoom: -2,
            maxZoom: 2
        });

        // Calculate bounds based on image dimensions
        const bounds = [[0, 0], [imageHeight, imageWidth]];
        
        // Add image overlay
        L.imageOverlay(imageUrl, bounds).addTo(map);
        
        // Fit map to bounds
        map.fitBounds(bounds);

        // Store map reference
        maps[mapId] = map;
        markers[mapId] = {};
        
        return true;
    } catch (error) {
        console.error('Error initializing inspection map:', error);
        return false;
    }
}

function getMarkerIcon(status) {
    const colors = {
        'OK': '#10b981',          // Green
        'Issue': '#ef4444',        // Red
        'NotAccessible': '#f59e0b', // Yellow
        'NotChecked': '#6b7280'    // Gray
    };
    
    const color = colors[status] || colors['NotChecked'];
    
    const svgIcon = `
        <svg width="25" height="41" viewBox="0 0 25 41" xmlns="http://www.w3.org/2000/svg">
            <g fill="none" fill-rule="evenodd">
                <path d="M12.5 0C5.596 0 0 5.596 0 12.5c0 1.886.415 3.674 1.16 5.28l.01.022.018.039L12.5 41l11.312-23.159.018-.039.01-.022A12.423 12.423 0 0 0 25 12.5C25 5.596 19.404 0 12.5 0z" fill="${color}"/>
                <circle fill="#fff" cx="12.5" cy="12.5" r="7"/>
            </g>
        </svg>
    `;
    
    return L.divIcon({
        html: svgIcon,
        className: 'custom-marker',
        iconSize: [25, 41],
        iconAnchor: [12.5, 41],
        popupAnchor: [0, -41]
    });
}

export function addInspectionMarker(mapId, x, y, imageWidth, imageHeight, objectId, icon, name, description, status) {
    try {
        if (!maps[mapId]) {
            console.error('Map not found:', mapId);
            return false;
        }

        const map = maps[mapId];
        
        // Convert normalized coordinates (0-1) to pixel coordinates
        const pixelY = y * imageHeight;
        const pixelX = x * imageWidth;

        // Create marker with custom icon
        const markerIcon = getMarkerIcon(status);
        const marker = L.marker([pixelY, pixelX], {
            icon: markerIcon,
            title: name
        });

        // Add popup with object info
        const popupContent = `
            <div style="min-width: 150px;">
                <strong>${icon} ${name}</strong><br/>
                <small>${description}</small><br/>
                <span style="margin-top: 8px; display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: bold; background-color: ${getStatusColor(status)}; color: white;">
                    ${getStatusLabel(status)}
                </span>
            </div>
        `;
        marker.bindPopup(popupContent);
        
        // Add to map
        marker.addTo(map);

        // Store object ID and status on marker
        marker.objectId = objectId;
        marker.objectStatus = status;
        
        // Store object data for later use when updating
        marker.objectData = { icon, name, description };

        // Store marker reference
        if (!markers[mapId]) {
            markers[mapId] = {};
        }
        markers[mapId][objectId] = marker;

        return true;
    } catch (error) {
        console.error('Error adding inspection marker:', error);
        return false;
    }
}

function getStatusColor(status) {
    const colors = {
        'OK': '#10b981',
        'Issue': '#ef4444',
        'NotAccessible': '#f59e0b',
        'NotChecked': '#6b7280'
    };
    return colors[status] || colors['NotChecked'];
}

function getStatusLabel(status) {
    const labels = {
        'OK': '✓ OK',
        'Issue': '⚠ Issue',
        'NotAccessible': '🔒 Not Accessible',
        'NotChecked': 'Not Checked'
    };
    return labels[status] || 'Not Checked';
}

export function onMarkerClick(mapId, dotnetHelper) {
    try {
        if (!maps[mapId]) {
            console.error('Map not found:', mapId);
            return false;
        }

        const map = maps[mapId];
        
        // Add click handler to all markers
        map.eachLayer(function(layer) {
            if (layer instanceof L.Marker && layer.objectId) {
                layer.off('click'); // Remove any existing handlers
                layer.on('click', function(e) {
                    // Stop event propagation to prevent map click
                    L.DomEvent.stopPropagation(e);
                    dotnetHelper.invokeMethodAsync('OnObjectClicked', layer.objectId);
                });
            }
        });

        return true;
    } catch (error) {
        console.error('Error setting up marker click:', error);
        return false;
    }
}

export function updateMarkerStatus(mapId, objectId, newStatus) {
    try {
        if (!markers[mapId] || !markers[mapId][objectId]) {
            console.error('Marker not found:', mapId, objectId);
            return false;
        }

        const marker = markers[mapId][objectId];
        
        // Update icon
        marker.setIcon(getMarkerIcon(newStatus));
        
        // Update status
        marker.objectStatus = newStatus;
        
        // Update popup content by recreating it
        // Store the original data on the marker
        if (marker.objectData) {
            const { icon, name, description } = marker.objectData;
            const popupContent = `
                <div style="min-width: 150px;">
                    <strong>${icon} ${name}</strong><br/>
                    <small>${description}</small><br/>
                    <span style="margin-top: 8px; display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: bold; background-color: ${getStatusColor(newStatus)}; color: white;">
                        ${getStatusLabel(newStatus)}
                    </span>
                </div>
            `;
            marker.setPopupContent(popupContent);
        }

        return true;
    } catch (error) {
        console.error('Error updating marker status:', error);
        return false;
    }
}
