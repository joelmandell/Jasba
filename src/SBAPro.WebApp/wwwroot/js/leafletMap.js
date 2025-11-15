// Leaflet Map Manager for SBA Pro
let maps = {};

export function initializeMap(mapId, imageUrl, imageWidth, imageHeight) {
    try {
        // Clean up existing map if any
        if (maps[mapId]) {
            maps[mapId].remove();
        }

        // Create map with simple coordinate system and no attribution control
        const map = L.map(mapId, {
            crs: L.CRS.Simple,
            minZoom: -2,
            maxZoom: 2,
            attributionControl: false // Remove the Leaflet attribution icon
        });

        // Calculate bounds based on image dimensions
        const bounds = [[0, 0], [imageHeight, imageWidth]];
        
        // Add image overlay if image URL is provided
        if (imageUrl) {
            L.imageOverlay(imageUrl, bounds).addTo(map);
        } else {
            // Create a simple grid background when no image is provided
            const canvas = document.createElement('canvas');
            canvas.width = imageWidth;
            canvas.height = imageHeight;
            const ctx = canvas.getContext('2d');
            
            // Fill with light gray background
            ctx.fillStyle = '#f5f5f5';
            ctx.fillRect(0, 0, imageWidth, imageHeight);
            
            // Draw grid
            ctx.strokeStyle = '#e0e0e0';
            ctx.lineWidth = 1;
            const gridSize = 50;
            
            for (let x = 0; x <= imageWidth; x += gridSize) {
                ctx.beginPath();
                ctx.moveTo(x, 0);
                ctx.lineTo(x, imageHeight);
                ctx.stroke();
            }
            
            for (let y = 0; y <= imageHeight; y += gridSize) {
                ctx.beginPath();
                ctx.moveTo(0, y);
                ctx.lineTo(imageWidth, y);
                ctx.stroke();
            }
            
            // Add text in the center
            ctx.fillStyle = '#999';
            ctx.font = '20px Arial';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText('No floor plan image - Click to place objects', imageWidth / 2, imageHeight / 2);
            
            // Use canvas as image overlay
            const canvasUrl = canvas.toDataURL();
            L.imageOverlay(canvasUrl, bounds).addTo(map);
        }
        
        // Fit map to bounds
        map.fitBounds(bounds);

        // Store map reference
        maps[mapId] = map;
        
        return true;
    } catch (error) {
        console.error('Error initializing map:', error);
        return false;
    }
}

export function addMarker(mapId, x, y, imageWidth, imageHeight, objectId, objectType, description) {
    try {
        if (!maps[mapId]) {
            console.error('Map not found:', mapId);
            return false;
        }

        const map = maps[mapId];
        
        // Convert normalized coordinates (0-1) to pixel coordinates
        const pixelY = y * imageHeight;
        const pixelX = x * imageWidth;

        // Create marker
        const marker = L.marker([pixelY, pixelX], {
            title: description,
            alt: objectType
        });

        // Add popup
        marker.bindPopup(`<strong>${objectType}</strong><br/>${description}`);
        
        // Add to map
        marker.addTo(map);

        // Store object ID on marker for later reference
        marker.objectId = objectId;

        return true;
    } catch (error) {
        console.error('Error adding marker:', error);
        return false;
    }
}

export function onMapClick(mapId, dotnetHelper, imageWidth, imageHeight) {
    try {
        if (!maps[mapId]) {
            console.error('Map not found:', mapId);
            return false;
        }

        const map = maps[mapId];
        
        map.on('click', function(e) {
            const lat = e.latlng.lat; // This is Y in pixel coordinates
            const lng = e.latlng.lng; // This is X in pixel coordinates
            
            // Convert to normalized coordinates (0-1)
            const normalizedY = lat / imageHeight;
            const normalizedX = lng / imageWidth;
            
            // Call back to .NET
            dotnetHelper.invokeMethodAsync('OnMapClicked', normalizedX, normalizedY);
        });

        return true;
    } catch (error) {
        console.error('Error setting up map click:', error);
        return false;
    }
}

export function clearMarkers(mapId) {
    try {
        if (!maps[mapId]) {
            console.error('Map not found:', mapId);
            return false;
        }

        const map = maps[mapId];
        
        // Remove all markers
        map.eachLayer(function(layer) {
            if (layer instanceof L.Marker) {
                map.removeLayer(layer);
            }
        });

        return true;
    } catch (error) {
        console.error('Error clearing markers:', error);
        return false;
    }
}
