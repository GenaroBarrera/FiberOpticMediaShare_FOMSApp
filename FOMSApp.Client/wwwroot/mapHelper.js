// This file holds functions that C# can call to control the map

// Function to initialize the map
export function initMap(elementId) { 
    // 1. Create the map and center it on Austin, TX (or your default location)
    var map = L.map(elementId).setView([30.2672, -97.7431], 13); // Latitude, Longitude, Zoom level

    // 2. Add the "Tile Layer" (the actual map images from OpenStreetMap)
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map); // Add the tile layer to the map

    return map; // Return the map object to C#
}

// Function to add a marker to the map
export function addMarker(map, lat, lng, popupText) {
    L.marker([lat, lng]).addTo(map)
        .bindPopup(popupText); // Add a popup to the marker
}

// Function to add a small circle marker (Dot)
export function addCircle(map, lat, lng, color, popupText) {
    var circleMarker = L.circleMarker([lat, lng], {
        color: 'black',       // Border color
        fillColor: color,     // Inside color (e.g., 'green')
        fillOpacity: 0.8,
        radius: 6             // Size of the dot (smaller than a pin)
    }).addTo(map);

    if (popupText) {
        circleMarker.bindPopup(popupText);
    }
    
    return circleMarker;
}