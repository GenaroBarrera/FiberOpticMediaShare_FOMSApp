// This file holds functions that C# can call to control the map

// Global variable to track if we're in delete mode
var isDeleteMode = false;

// Function to set the delete mode state
export function setDeleteMode(enabled) {
    isDeleteMode = enabled;
}

// Function to initialize the map
export function initMap(elementId) { 
    // 1. Create the map and center it on San Antonio, TX
    var map = L.map(elementId).setView([29.54248, -98.73548], 18); // Latitude, Longitude, Zoom level (18 for street-level detail)

    // 2. Add the "Tile Layer" (the actual map images from OpenStreetMap)
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map); // Add the tile layer to the map

    return map; // Return the map object to C#
}

// Function to create a custom colored icon for vault markers
// Returns a Leaflet DivIcon configured with the specified color
function createColoredIcon(color) {
    // Map color names to hex values for better browser compatibility
    var colorMap = {
        'Blue': '#0066CC',
        'Brown': '#8B4513',
        'Gray': '#808080',
        'Green': '#28A745',
        'Red': '#DC3545'
    };
    
    // Get hex color or use the provided color if it's already a hex code
    var hexColor = colorMap[color] || color || '#0066CC';
    
    // Create an SVG pin marker with the specified color
    // This creates a classic map pin shape: circular head with a triangular point
    var svgIcon = '<svg width="25" height="35" viewBox="0 0 25 35" xmlns="http://www.w3.org/2000/svg">' +
        // Pin head (circle)
        '<circle cx="12.5" cy="12.5" r="10" fill="' + hexColor + '" stroke="white" stroke-width="2"/>' +
        // Pin point (triangle)
        '<path d="M 12.5 22.5 L 3 35 L 22 35 Z" fill="' + hexColor + '" stroke="white" stroke-width="1"/>' +
        '</svg>';
    
    return L.divIcon({
        html: svgIcon,
        className: 'custom-colored-marker', // Custom class for styling if needed (no default Leaflet styles)
        iconSize: [25, 35],
        iconAnchor: [12.5, 35], // Anchor point at the bottom center of the pin
        popupAnchor: [0, -35] // Position popup above the marker
    });
}

// Function to add a marker to the map with a custom color
// Returns the marker so it can be stored and deleted later
export function addMarker(map, lat, lng, popupText, entityId, dotNetReference, color) {
    // Create a custom colored icon if color is provided, otherwise use default blue
    var icon = color ? createColoredIcon(color) : undefined;
    
    // Make the marker draggable so users can move it to a new location
    var marker = L.marker([lat, lng], { icon: icon, draggable: true }).addTo(map);
    marker.bindPopup(popupText); // Add a popup to the marker
    
    // If entityId and dotNetReference are provided, add click handler for deletion and drag handler for moving
    if (entityId && dotNetReference) {
        // Override the default click behavior to handle delete mode
        marker.off('click'); // Remove default click handler (which opens popup)
        marker.on('click', function(e) {
            // If in delete mode, prevent popup from opening and handle deletion
            if (isDeleteMode) {
                L.DomEvent.stop(e); // Stop Leaflet event propagation
                marker.closePopup(); // Close popup if it's already open
                // Call C# method to handle the click (for deletion mode)
                dotNetReference.invokeMethodAsync('OnEntityClick', 'vault', entityId);
            } else {
                // If not in delete mode, open the popup manually
                marker.openPopup();
            }
        });
        
        // Handle drag end event to update the location in the database
        marker.on('dragend', function(e) {
            var newLat = marker.getLatLng().lat;
            var newLng = marker.getLatLng().lng;
            // Call C# method to update the location
            dotNetReference.invokeMethodAsync('OnMarkerDragEnd', 'vault', entityId, newLat, newLng);
        });
    }
    
    return marker;
}

// Function to create a custom colored icon for midpoint markers
// Returns a Leaflet DivIcon configured with the specified color
// Midpoints use a square/diamond shape to distinguish them from vault pin markers
function createColoredMidpointIcon(color) {
    // Map color names to hex values for better browser compatibility
    var colorMap = {
        'Black': '#000000',
        'LightGray': '#D3D3D3',
        'LightGreen': '#90EE90',
        'LightCoral': '#F08080'
    };
    
    // Get hex color or use the provided color if it's already a hex code
    var hexColor = colorMap[color] || color || '#000000';
    
    // Create an SVG square/diamond marker with the specified color
    // This creates a distinct shape from vault pins (which are circular pins)
    var svgIcon = '<svg width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">' +
        // Diamond/square shape rotated 45 degrees
        '<rect x="5" y="5" width="10" height="10" transform="rotate(45 10 10)" ' +
        'fill="' + hexColor + '" stroke="white" stroke-width="2"/>' +
        '</svg>';
    
    return L.divIcon({
        html: svgIcon,
        className: 'custom-colored-midpoint-marker', // Custom class for styling
        iconSize: [20, 20],
        iconAnchor: [10, 10], // Anchor point at the center
        popupAnchor: [0, -10] // Position popup above the marker
    });
}

// Function to add a small circle marker (Dot)
// Returns the circle marker so it can be stored and deleted later
// Updated to use custom colored icons based on status
export function addCircle(map, lat, lng, color, popupText, entityId, dotNetReference) {
    // Create a custom colored icon if color is provided, otherwise use default circle marker
    var icon = color ? createColoredMidpointIcon(color) : undefined;
    
    // Use circle marker if no custom icon, otherwise use marker with custom icon
    var circleMarker;
    if (icon) {
        // Use marker with custom icon for better visual distinction
        // Make the marker draggable so users can move it to a new location
        circleMarker = L.marker([lat, lng], { icon: icon, draggable: true }).addTo(map);
    } else {
        // Fallback to circle marker if no color specified
        // Note: circleMarker doesn't support draggable directly, so we'll use a marker instead
        circleMarker = L.marker([lat, lng], { 
            draggable: true,
            icon: L.divIcon({
                className: 'custom-circle-marker',
                html: '<div style="width: 12px; height: 12px; border-radius: 50%; background-color: ' + (color || 'green') + '; border: 2px solid black;"></div>',
                iconSize: [12, 12],
                iconAnchor: [6, 6]
            })
        }).addTo(map);
    }

    if (popupText) {
        circleMarker.bindPopup(popupText);
    }
    
    // If entityId and dotNetReference are provided, add click handler for deletion and drag handler for moving
    if (entityId && dotNetReference) {
        // Override the default click behavior to handle delete mode
        circleMarker.off('click'); // Remove default click handler (which opens popup)
        circleMarker.on('click', function(e) {
            // If in delete mode, prevent popup from opening and handle deletion
            if (isDeleteMode) {
                L.DomEvent.stop(e); // Stop Leaflet event propagation
                circleMarker.closePopup(); // Close popup if it's already open
                // Call C# method to handle the click (for deletion mode)
                dotNetReference.invokeMethodAsync('OnEntityClick', 'midpoint', entityId);
            } else {
                // If not in delete mode, open the popup manually
                circleMarker.openPopup();
            }
        });
        
        // Handle drag end event to update the location in the database
        circleMarker.on('dragend', function(e) {
            var newLat = circleMarker.getLatLng().lat;
            var newLng = circleMarker.getLatLng().lng;
            // Call C# method to update the location
            dotNetReference.invokeMethodAsync('OnMarkerDragEnd', 'midpoint', entityId, newLat, newLng);
        });
    }
    
    return circleMarker;
}

// Function to add a polyline to the map
// Returns the polyline so it can be stored and deleted later
export function addPolyline(map, coordinates, color, popupText, entityId, dotNetReference) {
    var polyline = L.polyline(coordinates, {
        color: color,
        weight: 4,
        opacity: 0.8
    }).addTo(map);

    if (popupText) {
        polyline.bindPopup(popupText);
    }
    
    // If entityId and dotNetReference are provided, add click handler for deletion
    if (entityId && dotNetReference) {
        polyline.on('click', function() {
            // Call C# method to handle the click (for deletion mode)
            dotNetReference.invokeMethodAsync('OnEntityClick', 'cable', entityId);
        });
    }
    
    return polyline;
}

// Function to add a click event listener to the map that calls back to C#
// dotNetReference: The .NET object reference to invoke the OnMapClick method
export function addClickEventListener(map, dotNetReference) {
    map.on('click', function(e) {
        // e.latlng contains the latitude and longitude of the click
        // Call the C# method OnMapClick with the coordinates
        dotNetReference.invokeMethodAsync('OnMapClick', e.latlng.lat, e.latlng.lng);
    });
}

// Function to remove a layer (marker, polyline, circle, etc.) from the map
export function removeLayer(map, layer) {
    map.removeLayer(layer);
}

// Function to update a marker's popup content
// Used after moving a marker to update the coordinates displayed in the popup
export function updateMarkerPopup(marker, newPopupText) {
    marker.setPopupContent(newPopupText);
}