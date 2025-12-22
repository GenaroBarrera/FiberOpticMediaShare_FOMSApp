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

// Function to add a marker to the map
// Returns the marker so it can be stored and deleted later
export function addMarker(map, lat, lng, popupText, entityId, dotNetReference) {
    var marker = L.marker([lat, lng]).addTo(map);
    marker.bindPopup(popupText); // Add a popup to the marker
    
    // If entityId and dotNetReference are provided, add click handler for deletion
    if (entityId && dotNetReference) {
        marker.on('click', function(e) {
            // If in delete mode, prevent popup from opening and handle deletion
            if (isDeleteMode) {
                e.originalEvent.stopPropagation(); // Prevent event bubbling
                e.originalEvent.preventDefault(); // Prevent default behavior
                marker.closePopup(); // Close popup if it's already open
                // Call C# method to handle the click (for deletion mode)
                dotNetReference.invokeMethodAsync('OnEntityClick', 'vault', entityId);
                return false; // Prevent further event handling
            }
            // If not in delete mode, let the popup open normally (default behavior)
        });
    }
    
    return marker;
}

// Function to add a small circle marker (Dot)
// Returns the circle marker so it can be stored and deleted later
export function addCircle(map, lat, lng, color, popupText, entityId, dotNetReference) {
    var circleMarker = L.circleMarker([lat, lng], {
        color: 'black',       // Border color
        fillColor: color,     // Inside color (e.g., 'green')
        fillOpacity: 0.8,
        radius: 6             // Size of the dot (smaller than a pin)
    }).addTo(map);

    if (popupText) {
        circleMarker.bindPopup(popupText);
    }
    
    // If entityId and dotNetReference are provided, add click handler for deletion
    if (entityId && dotNetReference) {
        circleMarker.on('click', function(e) {
            // If in delete mode, prevent popup from opening and handle deletion
            if (isDeleteMode) {
                e.originalEvent.stopPropagation(); // Prevent event bubbling
                e.originalEvent.preventDefault(); // Prevent default behavior
                circleMarker.closePopup(); // Close popup if it's already open
                // Call C# method to handle the click (for deletion mode)
                dotNetReference.invokeMethodAsync('OnEntityClick', 'midpoint', entityId);
                return false; // Prevent further event handling
            }
            // If not in delete mode, let the popup open normally (default behavior)
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