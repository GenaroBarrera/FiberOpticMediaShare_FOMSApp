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