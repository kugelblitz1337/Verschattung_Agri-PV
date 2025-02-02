import numpy as np
import matplotlib.pyplot as plt
from datetime import datetime, timedelta
from pytz import timezone
from pysolar.solar import get_altitude, get_azimuth

def calculate_shadow_for_corners(latitude, longitude, width, height, ground_clearance, orientation, year=2023, tz_name="Europe/Berlin", area_limit=1000):
    """
    Berechnet die verschatteten Punkte einer senkrechten Solarplatte (alle Ecken) über ein Jahr hinweg.

    Parameters:
    - latitude, longitude: Standort der Solaranlage.
    - width, height: Breite und Höhe der Solarplatte (in Metern).
    - ground_clearance: Abstand der Unterkante der Platte vom Boden (in Metern).
    - orientation: Orientierung der Platte (Azimutwinkel in Grad, 0 = Nord, 180 = Süd).
    - year: Jahr für die Berechnung.
    - tz_name: Zeitzone des Standorts (z. B. "Europe/Berlin").
    - area_limit: Begrenzung der Fläche (halbe Kantenlänge in Metern).

    Returns:
    - shadow_points: Liste von Schattenpunkten (alle Ecken, alle Zeitpunkte).
    """
    tz = timezone(tz_name)

    # Zeiten generieren (z. B. alle 10 Tage, alle 15 Minuten)
    times = [
        tz.localize(datetime(year, month, day, hour, minute))
        for month in range(1, 13)
        for day in range(1, 32)
        for hour in range(6, 19)  # Sonnenaufgang bis Sonnenuntergang
        for minute in range(0, 60)  # Alle 15 Minuten
        if day <= (datetime(year, month, 1) + timedelta(days=31)).replace(day=1).day
    ]

    # Eckpunkte der Platte relativ zum Mittelpunkt
    half_width = width / 2
    half_height = height / 2
    corners = [
        (half_width, ground_clearance + height),  # Oben rechts
        (-half_width, ground_clearance + height),  # Oben links
        (-half_width, ground_clearance),  # Unten links
        (half_width, ground_clearance),  # Unten rechts
    ]

    shadow_points = []

    for time in times:
        # Berechne Sonnenhöhe und Azimut
        altitude = get_altitude(latitude, longitude, time)
        azimuth = get_azimuth(latitude, longitude, time)
        
        if altitude > 0:  # Nur wenn die Sonne über dem Horizont steht
            for corner_x, corner_y in corners:
                # Schattenlänge und Richtung berechnen
                shadow_length = corner_y / np.tan(np.radians(altitude))
                shadow_angle = (azimuth - orientation) % 360
                shadow_x = corner_x + shadow_length * np.sin(np.radians(shadow_angle))
                shadow_y = shadow_length * np.cos(np.radians(shadow_angle))
                
                # Begrenzung der Fläche
                if -area_limit <= shadow_x <= area_limit and -area_limit <= shadow_y <= area_limit:
                    shadow_points.append((shadow_x, shadow_y))
    
    return shadow_points

def plot_shadow_heatmap(shadow_points, area_limit=1000, resolution=500):
    """
    Visualisiert die Schattenintensität als Heatmap.

    Parameters:
    - shadow_points: Liste von Schattenpunkten.
    - area_limit: Begrenzung der Fläche (halbe Kantenlänge in Metern).
    - resolution: Anzahl der Rasterzellen entlang einer Achse.
    """
    x_coords, y_coords = zip(*shadow_points)

    # Erstelle ein 2D-Histogramm (Heatmap) für die Schattenintensität
    heatmap, xedges, yedges = np.histogram2d(
        x_coords, y_coords, 
        bins=resolution, 
        range=[[-area_limit, area_limit], [-area_limit, area_limit]]
    )

    # Zeichne die Heatmap
    plt.figure(figsize=(12, 12))
    plt.imshow(
        np.log1p(heatmap.T),  # Logarithmische Skalierung für bessere Farbkontraste
        extent=[-area_limit, area_limit, -area_limit, area_limit],
        origin='lower',
        cmap='viridis',
        interpolation='nearest',
    )
    plt.colorbar(label="Schattenintensität (logarithmiert)")
    plt.title(f"Schattenintensität über ein Jahr ({2 * area_limit} m x {2 * area_limit} m Fläche)")
    plt.xlabel("Position X (Meter)")
    plt.ylabel("Position Y (Meter)")
    plt.grid(color="white", alpha=0.2)
    plt.show()

# Parameter der Solaranlage
latitude = 48.1351  # Beispiel: München
longitude = 11.5820
width = 2  # Breite der Solarplatte (in Metern)
height = 2.22  # Höhe der Solarplatte (in Metern)
ground_clearance = 0.8  # Abstand vom Boden zur Unterkante (in Metern)
orientation = 180  # Richtung Süden

# Einstellbare Parameter
area_limit = 100  # Begrenzung der Fläche (halbe Kantenlänge in Metern, 500 = 1 km x 1 km)
resolution = 200  # Auflösung der Heatmap (Anzahl Rasterzellen pro Achse)

# Berechnung und Visualisierung
shadow_points = calculate_shadow_for_corners(latitude, longitude, width, height, ground_clearance, orientation, area_limit=area_limit)
plot_shadow_heatmap(shadow_points, area_limit=area_limit, resolution=resolution)


#solarplatten-Lichtdurchlässigkeit 
