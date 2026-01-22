// This file holds functions that C# can call to control the map

// Global variable to track if we're in delete mode
var isDeleteMode = false;

// Global variable to track if we're in select mode
var isSelectMode = false;

// Function to set the delete mode state
export function setDeleteMode(enabled) {
    isDeleteMode = enabled;
}

// Function to set the select mode state
export function setSelectMode(enabled) {
    isSelectMode = enabled;
}

// Function to initialize the map
export function initMap(elementId) { 
    // 1. Create the map and center it on San Antonio, TX
    var map = L.map(elementId).setView([29.54248, -98.73548], 18); // Latitude, Longitude, Zoom level (18 for street-level detail)

    // 2. Create the default map layer (OpenStreetMap)
    var defaultLayer = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    });

    // 3. Create the satellite imagery base layer (Esri World Imagery - free, no API key required)
    var satelliteLayer = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.esri.com/">Esri</a> &mdash; Source: Esri, Maxar, GeoEye, Earthstar Geographics, CNES/Airbus DS, USDA, USGS, AeroGRID, IGN, and the GIS User Community'
    });

    // 4. Create road labels and street overlay layer for satellite view
    // This provides road names, street outlines, and sidewalk information
    var roadLabelsLayer = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/Reference/World_Transportation/MapServer/tile/{z}/{y}/{x}', {
        maxZoom: 19,
        opacity: 0.7, // Semi-transparent so satellite imagery shows through
        attribution: '&copy; <a href="https://www.esri.com/">Esri</a>'
    });

    // 5. Add default layer initially (user can toggle to satellite)
    defaultLayer.addTo(map);

    // Store layer references for layer switching
    map._defaultLayer = defaultLayer;
    map._satelliteLayer = satelliteLayer;
    map._roadLabelsLayer = roadLabelsLayer;
    map._currentMapType = 'default'; // Track current map type

    return map; // Return the map object to C#
}

// Function to switch between default and satellite map views
export function toggleMapView(map, useSatellite) {
    if (!map) return;

    if (useSatellite) {
        // Switch to satellite view
        if (map._currentMapType !== 'satellite') {
            // Remove default layer
            if (map._defaultLayer) {
                map.removeLayer(map._defaultLayer);
            }
            // Add satellite layer
            if (map._satelliteLayer) {
                map._satelliteLayer.addTo(map);
            }
            // Add road labels overlay
            if (map._roadLabelsLayer) {
                map._roadLabelsLayer.addTo(map);
            }
            map._currentMapType = 'satellite';
        }
    } else {
        // Switch to default view
        if (map._currentMapType !== 'default') {
            // Remove satellite layers
            if (map._satelliteLayer) {
                map.removeLayer(map._satelliteLayer);
            }
            if (map._roadLabelsLayer) {
                map.removeLayer(map._roadLabelsLayer);
            }
            // Add default layer
            if (map._defaultLayer) {
                map._defaultLayer.addTo(map);
            }
            map._currentMapType = 'default';
        }
    }
    
    // Invalidate size to ensure map renders correctly after layer change
    map.invalidateSize();
}

// Function to create a custom colored icon for vault markers
// Returns a Leaflet DivIcon configured with the specified color
// Vault markers are triangles pointing down (point at bottom) with white outline, slightly larger than midpoint markers
// If isSelected is true, adds a highlight border/glow effect
function createColoredIcon(color, isSelected) {
    isSelected = isSelected || false;
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
    
    // Create an SVG triangle marker with white outline, pointing down (point at bottom)
    // Size: 24x24 (slightly larger than midpoint which is 20x20, but smaller than previous 26x26)
    // Triangle points downward with point at the bottom
    // If selected, add a yellow highlight border
    var strokeColor = isSelected ? '#FFD700' : 'white'; // Gold/yellow for selected, white for normal
    var strokeWidth = isSelected ? '3' : '2'; // Thicker border when selected
    var svgIcon = '<svg width="24" height="24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">' +
        // Triangle pointing down (point at bottom, base at top)
        '<path d="M 12 22 L 2 2 L 22 2 Z" ' +
        'fill="' + hexColor + '" stroke="' + strokeColor + '" stroke-width="' + strokeWidth + '"/>' +
        '</svg>';
    
    return L.divIcon({
        html: svgIcon,
        className: 'custom-colored-marker', // Custom class for styling if needed (no default Leaflet styles)
        iconSize: [24, 24],
        iconAnchor: [12, 22], // Anchor point at the bottom center of the triangle (the point)
        popupAnchor: [0, -12] // Position popup above the marker
    });
}

// Function to add a marker to the map with a custom color
// Returns the marker so it can be stored and deleted later
export function addMarker(map, lat, lng, popupText, entityId, dotNetReference, color, isSelected) {
    isSelected = isSelected || false;
    // Create a custom colored icon if color is provided, otherwise use default blue
    var icon = color ? createColoredIcon(color, isSelected) : undefined;
    
    // Make the marker draggable so users can move it to a new location
    var marker = L.marker([lat, lng], { icon: icon, draggable: true }).addTo(map);
    marker.bindPopup(popupText); // Add a popup to the marker
    
    // Add event listener for Edit button clicks in the popup
    marker.on('popupopen', function() {
        // Wait for popup to be added to DOM, then attach click handler to Edit button
        setTimeout(function() {
            var editButton = document.getElementById('editVault_' + entityId);
            if (editButton && dotNetReference) {
                editButton.onclick = function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    dotNetReference.invokeMethodAsync('OpenEditModal', 'vault', entityId);
                };
            }
        }, 100);
    });
    
    // If entityId and dotNetReference are provided, add click handler for deletion and drag handler for moving
    if (entityId && dotNetReference) {
        // Override the default click behavior to handle delete and select modes
        marker.off('click'); // Remove default click handler (which opens popup)
        marker.on('click', function(e) {
            // If in delete mode, prevent popup from opening and handle deletion
            if (isDeleteMode) {
                L.DomEvent.stop(e); // Stop Leaflet event propagation
                marker.closePopup(); // Close popup if it's already open
                // Call C# method to handle the click (for deletion mode)
                dotNetReference.invokeMethodAsync('OnEntityClick', 'vault', entityId);
            } else if (isSelectMode) {
                // If in select mode, prevent popup from opening and handle selection
                L.DomEvent.stop(e); // Stop Leaflet event propagation
                marker.closePopup(); // Close popup if it's already open
                // Call C# method to handle the click (for selection mode)
                dotNetReference.invokeMethodAsync('OnEntityClick', 'vault', entityId);
            } else {
                // If not in delete or select mode, open the popup manually
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
// If isSelected is true, adds a highlight border/glow effect
function createColoredMidpointIcon(color, isSelected) {
    isSelected = isSelected || false;
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
    // If selected, add a yellow highlight border
    var strokeColor = isSelected ? '#FFD700' : 'white'; // Gold/yellow for selected, white for normal
    var strokeWidth = isSelected ? '3' : '2'; // Thicker border when selected
    var svgIcon = '<svg width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">' +
        // Diamond/square shape rotated 45 degrees
        '<rect x="5" y="5" width="10" height="10" transform="rotate(45 10 10)" ' +
        'fill="' + hexColor + '" stroke="' + strokeColor + '" stroke-width="' + strokeWidth + '"/>' +
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
export function addCircle(map, lat, lng, color, popupText, entityId, dotNetReference, isSelected) {
    isSelected = isSelected || false;
    // Create a custom colored icon if color is provided, otherwise use default circle marker
    var icon = color ? createColoredMidpointIcon(color, isSelected) : undefined;
    
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
    
    // Add event listener for Edit button clicks in the popup
    circleMarker.on('popupopen', function() {
        // Wait for popup to be added to DOM, then attach click handler to Edit button
        setTimeout(function() {
            var editButton = document.getElementById('editMidpoint_' + entityId);
            if (editButton && dotNetReference) {
                editButton.onclick = function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    dotNetReference.invokeMethodAsync('OpenEditModal', 'midpoint', entityId);
                };
            }
        }, 100);
    });
    
    // If entityId and dotNetReference are provided, add click handler for deletion and drag handler for moving
    if (entityId && dotNetReference) {
        // Override the default click behavior to handle delete and select modes
        circleMarker.off('click'); // Remove default click handler (which opens popup)
        circleMarker.on('click', function(e) {
            // If in delete mode, prevent popup from opening and handle deletion
            if (isDeleteMode) {
                L.DomEvent.stop(e); // Stop Leaflet event propagation
                circleMarker.closePopup(); // Close popup if it's already open
                // Call C# method to handle the click (for deletion mode)
                dotNetReference.invokeMethodAsync('OnEntityClick', 'midpoint', entityId);
            } else if (isSelectMode) {
                // If in select mode, prevent popup from opening and handle selection
                L.DomEvent.stop(e); // Stop Leaflet event propagation
                circleMarker.closePopup(); // Close popup if it's already open
                // Call C# method to handle the click (for selection mode)
                dotNetReference.invokeMethodAsync('OnEntityClick', 'midpoint', entityId);
            } else {
                // If not in delete or select mode, open the popup manually
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

// Function to show a layer (make it visible)
export function showLayer(map, layer) {
    if (layer && !map.hasLayer(layer)) {
        layer.addTo(map);
    }
}

// Function to hide a layer (make it invisible but keep it in memory)
export function hideLayer(map, layer) {
    if (layer && map.hasLayer(layer)) {
        map.removeLayer(layer);
    }
}

// Function to update a marker's popup content
// Used after moving a marker to update the coordinates displayed in the popup
export function updateMarkerPopup(marker, newPopupText) {
    marker.setPopupContent(newPopupText);
}

// Function to attach click handler to Edit button in popup
// This is needed because popup content is dynamically generated HTML
export function attachEditButtonHandler(marker, buttonId, entityType, entityId, dotNetReference) {
    // Wait for popup to be added to DOM, then attach click handler
    marker.on('popupopen', function() {
        setTimeout(function() {
            var editButton = document.getElementById(buttonId);
            if (editButton && dotNetReference) {
                // Remove any existing handler to avoid duplicates
                editButton.onclick = null;
                editButton.onclick = function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    dotNetReference.invokeMethodAsync('OpenEditModal', entityType, entityId);
                };
            }
        }, 100);
    });
    
    // Also try to attach immediately if popup is already open
    setTimeout(function() {
        var editButton = document.getElementById(buttonId);
        if (editButton && dotNetReference) {
            editButton.onclick = null;
            editButton.onclick = function(e) {
                e.preventDefault();
                e.stopPropagation();
                dotNetReference.invokeMethodAsync('OpenEditModal', entityType, entityId);
            };
        }
    }, 200);
}

// Function to navigate the map to a specific location
// Pans and zooms the map to center on the given coordinates
export function navigateToLocation(map, lat, lng) {
    // Use flyTo for smooth animation, or setView for instant navigation
    // flyTo provides a smooth animated transition to the new location
    map.flyTo([lat, lng], 18, {
        animate: true,
        duration: 1.0 // Animation duration in seconds
    });
}

// Global function to handle photo navigation in popups
// This needs to be in the global scope so it can be called from inline onclick handlers
window.navigatePhoto = function(containerId, direction) {
    var container = document.getElementById(containerId);
    if (!container) return;
    
    var photosAttr = container.getAttribute('data-photos');
    if (!photosAttr) return;
    
    // Decode HTML entities (file names are HTML encoded in the attribute)
    var tempDiv = document.createElement('div');
    tempDiv.innerHTML = photosAttr;
    var decodedPhotos = tempDiv.textContent || tempDiv.innerText || photosAttr;
    
    // Split photos
    var photos = decodedPhotos.split(',').filter(function(p) { return p.trim(); });
    if (photos.length === 0) return;
    
    var current = parseInt(container.getAttribute('data-current')) || 0;
    var baseUrlAttr = container.getAttribute('data-baseurl');
    
    // Decode baseUrl if it was HTML encoded
    tempDiv.innerHTML = baseUrlAttr || '/api/photos/file/';
    var baseUrl = tempDiv.textContent || tempDiv.innerText || (baseUrlAttr || '/api/photos/file/');
    
    // Update current index
    current += direction;
    if (current < 0) current = photos.length - 1;
    if (current >= photos.length) current = 0;
    
    // Update image src - encode filename for URL
    var img = document.getElementById('img_' + containerId);
    if (img) {
        var fileName = photos[current].trim();
        img.src = baseUrl + encodeURIComponent(fileName);
        img.alt = 'Photo ' + (current + 1) + ' of ' + photos.length;
    }
    
    // Update photo count display
    var countSpan = document.getElementById('photoCount_' + containerId);
    if (countSpan) {
        countSpan.textContent = (current + 1) + ' of ' + photos.length;
    }
    
    // Update data attribute
    container.setAttribute('data-current', current);
};

// Function to invalidate the map size, forcing Leaflet to recalculate dimensions
// This should be called after the container size changes (e.g., after DOM updates)
export function invalidateMapSize(map) {
    if (map) {
        map.invalidateSize();
    }
}

// Function to create a white circle marker for cable vertices
// These markers are visible at each point of the cable (similar to Google My Maps)
// Note: Marker is created but not added to map initially - will be shown when cable is selected or dragging
export function addCableVertexMarker(map, lat, lng) {
    // Create a white circle marker with a border
    // Don't add to map initially - will be shown/hidden based on selection state
    var marker = L.circleMarker([lat, lng], {
        radius: 6,
        fillColor: '#ffffff',
        color: '#333333',
        weight: 2,
        opacity: 1,
        fillOpacity: 1
    });
    // Note: Not calling .addTo(map) here - marker will be added when shown
    
    return marker;
}

// Function to add an editable polyline with drag support for the entire cable
// Returns the polyline so it can be stored and deleted later
export function addEditablePolyline(map, coordinates, color, popupText, entityId, dotNetReference) {
    // Map color names to hex values
    var colorMap = {
        'Black': '#000000',
        'Blue': '#0066CC',
        'Orange': '#FF6600',
        'Green': '#28A745',
        'Brown': '#8B4513',
        'Pink': '#FF69B4',
        'Teal': '#008080'
    };
    
    var hexColor = colorMap[color] || color || '#000000';
    
    var polyline = L.polyline(coordinates, {
        color: hexColor,
        weight: 4,
        opacity: 0.8
    }).addTo(map);

    if (popupText) {
        polyline.bindPopup(popupText);
    }
    
    // Store the original coordinates for drag calculation
    var originalLatLngs = coordinates.map(function(coord) {
        return L.latLng(coord.lat, coord.lng);
    });
    
    // Track drag state
    var isDragging = false;
    var dragStartMouseLatLng = null;
    
    // Store reference to vertex markers (will be set by makeVertexDraggable)
    polyline._vertexMarkers = [];
    
    // Add mousedown event to start dragging the entire cable
    polyline.on('mousedown', function(e) {
        if (!isDeleteMode) {
            isDragging = true;
            dragStartMouseLatLng = L.latLng(e.latlng.lat, e.latlng.lng);
            map.dragging.disable(); // Disable map dragging while dragging the cable
            L.DomEvent.stop(e); // Prevent event propagation
        }
    });
    
    // Add mousemove event to update polyline position while dragging
    var mousemoveHandler = function(e) {
        if (isDragging && !isDeleteMode) {
            var currentLatLng = e.latlng;
            var deltaLat = currentLatLng.lat - dragStartMouseLatLng.lat;
            var deltaLng = currentLatLng.lng - dragStartMouseLatLng.lng;
            
            // Update all coordinates
            var newLatLngs = originalLatLngs.map(function(latLng) {
                return L.latLng(latLng.lat + deltaLat, latLng.lng + deltaLng);
            });
            
            polyline.setLatLngs(newLatLngs);
            
            // Update vertex markers position
            if (polyline._vertexMarkers) {
                polyline._vertexMarkers.forEach(function(marker, index) {
                    if (marker && newLatLngs[index]) {
                        marker.setLatLng(newLatLngs[index]);
                    }
                });
            }
        }
    };
    
    map.on('mousemove', mousemoveHandler);
    
    // Store handler for cleanup
    polyline._mousemoveHandler = mousemoveHandler;
    
    // Add mouseup event to end dragging
    var mouseupHandler = function(e) {
        if (isDragging && !isDeleteMode) {
            isDragging = false;
            map.dragging.enable(); // Re-enable map dragging
            
            var currentLatLng = e.latlng;
            var deltaLat = currentLatLng.lat - dragStartMouseLatLng.lat;
            var deltaLng = currentLatLng.lng - dragStartMouseLatLng.lng;
            
            // Update original coordinates for next drag
            originalLatLngs = polyline.getLatLngs();
            
            // Call C# method to update the cable in the database
            if (dotNetReference && (Math.abs(deltaLat) > 0.000001 || Math.abs(deltaLng) > 0.000001)) {
                dotNetReference.invokeMethodAsync('OnCableDragEnd', entityId, deltaLat, deltaLng);
            }
        }
    };
    
    map.on('mouseup', mouseupHandler);
    
    // Store handler for cleanup
    polyline._mouseupHandler = mouseupHandler;
    
    // If entityId and dotNetReference are provided, add click handler for selection and deletion
    if (entityId && dotNetReference) {
        polyline.on('click', function(e) {
            if (isDeleteMode) {
                L.DomEvent.stop(e);
                // Call C# method to handle the click (for deletion mode)
                dotNetReference.invokeMethodAsync('OnEntityClick', 'cable', entityId);
            } else {
                // Not in delete mode - treat as selection to show/hide vertices
                dotNetReference.invokeMethodAsync('OnCableSelected', entityId);
            }
        });
    }
    
    // Store original coordinates on the polyline for vertex updates
    polyline._originalLatLngs = originalLatLngs;
    
    return polyline;
}

// Function to make a vertex marker draggable and connect it to a polyline
// When the vertex is dragged, it updates the polyline and calls C# to update the database
export function makeVertexDraggable(map, vertexMarker, cableId, vertexIndex, polyline, dotNetReference) {
    // Make the marker draggable (circleMarker doesn't support draggable option, so we need to use a different approach)
    // Convert to a marker for draggability
    var latLng = vertexMarker.getLatLng();
    var draggableMarker = L.marker(latLng, {
        draggable: true,
        icon: L.divIcon({
            className: 'cable-vertex-marker',
            html: '<div style="width: 12px; height: 12px; border-radius: 50%; background-color: #ffffff; border: 2px solid #333333; cursor: move;"></div>',
            iconSize: [12, 12],
            iconAnchor: [6, 6]
        })
    });
    // Note: Not adding to map initially - will be shown when cable is selected or dragging
    // This keeps vertices hidden by default
    
    // Remove the original circle marker (if it was added to map)
    if (map.hasLayer(vertexMarker)) {
        map.removeLayer(vertexMarker);
    }
    
    // Track the cable and vertex info
    draggableMarker._cableId = cableId;
    draggableMarker._vertexIndex = vertexIndex;
    draggableMarker._polyline = polyline;
    
    // Add to polyline's vertex markers array
    if (!polyline._vertexMarkers) {
        polyline._vertexMarkers = [];
    }
    polyline._vertexMarkers[vertexIndex] = draggableMarker;
    
    // Handle drag start - show vertex markers when dragging starts
    draggableMarker.on('dragstart', function(e) {
        map.dragging.disable(); // Disable map dragging while dragging the vertex
        // Show all vertex markers for this cable when dragging starts
        // This ensures all vertices are visible while editing
        if (polyline._vertexMarkers) {
            polyline._vertexMarkers.forEach(function(marker) {
                if (marker) {
                    if (!map.hasLayer(marker)) {
                        marker.addTo(map); // Add marker to map if not already visible
                    }
                }
            });
        }
        // Also ensure this marker is visible
        if (!map.hasLayer(draggableMarker)) {
            draggableMarker.addTo(map);
        }
        // Notify C# that dragging started so it can track selection state
        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('OnCableVertexDragStart', cableId);
        }
    });
    
    // Handle drag - update the polyline in real-time
    draggableMarker.on('drag', function(e) {
        if (polyline) {
            var newLatLng = draggableMarker.getLatLng();
            var latLngs = polyline.getLatLngs();
            
            // Update the coordinate at the vertex index
            if (latLngs && latLngs[vertexIndex] !== undefined) {
                latLngs[vertexIndex] = newLatLng;
                polyline.setLatLngs(latLngs);
            }
        }
    });
    
    // Handle drag end - update the database
    draggableMarker.on('dragend', function(e) {
        map.dragging.enable(); // Re-enable map dragging
        
        var newLatLng = draggableMarker.getLatLng();
        var newLat = newLatLng.lat;
        var newLng = newLatLng.lng;
        
        // Call C# method to update the cable vertex in the database
        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('OnCableVertexDragEnd', cableId, vertexIndex, newLat, newLng);
        }
        
        // After drag ends, hide vertices if cable is not selected
        // The C# method will handle showing/hiding based on selection state
        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('OnCableVertexDragEndComplete', cableId);
        }
    });
    
    // Prevent vertex marker clicks from triggering cable deletion
    draggableMarker.on('click', function(e) {
        if (isDeleteMode) {
            L.DomEvent.stop(e);
            // Don't delete on vertex click - only on cable line click
        }
    });
    
    return draggableMarker;
}

// Note: Selection is now handled directly by clicking markers/circles in Select mode
// The window.selectEntity function is no longer needed

// Function to update marker selection visual state
// This is called from C# to update the marker highlight after selection changes
export function setMarkerSelection(layer, isSelected) {
    if (!layer) return;
    
    // Get the current icon
    var currentIcon = layer.options.icon;
    if (!currentIcon || !currentIcon.options) return;
    
    // Get the color from the current icon HTML (extract from SVG)
    var currentHtml = currentIcon.options.html;
    if (!currentHtml) return;
    
    // Extract color from the SVG fill attribute
    var fillMatch = currentHtml.match(/fill="([^"]+)"/);
    if (!fillMatch) return;
    
    var color = fillMatch[1];
    
    // Create new icon with selection state
    var newIcon = createColoredIcon(color, isSelected);
    layer.setIcon(newIcon);
}

// Function to update circle marker selection visual state
// This is called from C# to update the marker highlight after selection changes
export function setCircleSelection(layer, isSelected) {
    if (!layer) return;
    
    // Get the current icon
    var currentIcon = layer.options.icon;
    if (!currentIcon || !currentIcon.options) return;
    
    // Get the color from the current icon HTML (extract from SVG)
    var currentHtml = currentIcon.options.html;
    if (!currentHtml) return;
    
    // Extract color from the SVG fill attribute
    var fillMatch = currentHtml.match(/fill="([^"]+)"/);
    if (!fillMatch) return;
    
    var color = fillMatch[1];
    
    // Create new icon with selection state
    var newIcon = createColoredMidpointIcon(color, isSelected);
    layer.setIcon(newIcon);
}

// Function to update polyline selection visual state (e.g., cables).
// This is called from C# to highlight/unhighlight a cable polyline for batch operations.
export function setPolylineSelection(layer, isSelected) {
    if (!layer) return;

    try {
        // Cache the original style once so we can restore it later
        if (!layer._originalStyle) {
            layer._originalStyle = {
                color: layer.options?.color,
                weight: layer.options?.weight,
                opacity: layer.options?.opacity,
                dashArray: layer.options?.dashArray
            };
        }

        if (isSelected) {
            // High-contrast highlight (gold + thicker line)
            layer.setStyle({
                color: '#FFD700',
                weight: Math.max(6, (layer._originalStyle.weight || 4) + 2),
                opacity: 1,
                dashArray: null
            });
        } else {
            // Restore original style
            layer.setStyle({
                color: layer._originalStyle.color,
                weight: layer._originalStyle.weight,
                opacity: layer._originalStyle.opacity,
                dashArray: layer._originalStyle.dashArray
            });
        }
    } catch (error) {
        console.warn('Error updating polyline selection style:', error);
    }
}

// Global function to download a file from base64 data with folder selection
// This needs to be in the global scope so it can be called from C# via JSInterop
// Uses File System Access API when available to let user choose save location
window.downloadFile = async function(fileName, base64Data) {
    try {
        // Convert base64 string to binary data
        var binaryString = atob(base64Data);
        var bytes = new Uint8Array(binaryString.length);
        for (var i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        
        // Create a Blob from the binary data
        var blob = new Blob([bytes], { type: 'application/zip' });
        
        // Check if File System Access API is supported (Chrome, Edge, etc.)
        if ('showSaveFilePicker' in window) {
            try {
                // Use File System Access API to show a file picker
                var fileHandle = await window.showSaveFilePicker({
                    suggestedName: fileName,
                    types: [{
                        description: 'ZIP files',
                        accept: {
                            'application/zip': ['.zip']
                        }
                    }]
                });
                
                // Write the file to the selected location
                var writable = await fileHandle.createWritable();
                await writable.write(blob);
                await writable.close();
                
                return; // Success - file saved to user-selected location
            } catch (error) {
                // User cancelled the file picker, or an error occurred
                if (error.name === 'AbortError') {
                    // User cancelled - this is fine, just return silently
                    return;
                }
                // If File System Access API fails, fall back to default download
                console.warn('File System Access API failed, falling back to default download:', error);
            }
        }
        
        // Fallback: Use traditional download method (default download folder)
        // This works in all browsers but doesn't allow folder selection
        var url = window.URL.createObjectURL(blob);
        
        // Create a temporary anchor element and trigger download
        var link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        
        // Clean up: remove the link and revoke the URL
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
    } catch (error) {
        console.error('Error downloading file:', error);
        alert('Failed to download file. Please try again.');
    }
};