/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package agri.pv_shadow_sim_java;

import java.awt.Color;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.util.ArrayList;
import javax.swing.JPanel;
import org.locationtech.jts.geom.Coordinate;
import org.locationtech.jts.geom.Geometry;
import org.locationtech.jts.geom.Point;
import org.locationtech.jts.geom.Polygon;

/**
 * Das {@code GeometryPanel} ist eine benutzerdefinierte JPanel-Implementierung,
 * die für die Darstellung von geografischen Geometrien (Polygone, Punkte)
 * verwendet wird. Es kann sowohl Umrisse als auch gefüllte Geometrien zeichnen,
 * und skaliert diese basierend auf den Bounding Boxen der Daten, um sie
 * responsiv innerhalb des Panels darzustellen.
 *
 * @author roesc
 */
public class GeometryPanel extends JPanel {

    // Listen zum Speichern der Geometrien und ihrer Farben
    private final ArrayList<Geometry> outlineGeometries = new ArrayList<>(); // Für Geometrien, die als Umriss gezeichnet werden sollen
    private final ArrayList<Color> outlineColors = new ArrayList<>(); // Farben für die Umrissgeometrien
    
    private final ArrayList<Geometry> filledGeometries = new ArrayList<>(); // Für Geometrien, die als gefüllte Flächen gezeichnet werden sollen (z.B. Heatmap)
    private final ArrayList<Color> filledColors = new ArrayList<>(); // Farben für die gefüllten Geometrien

    private AgriPVData data; // Referenz zum AgriPVData-Objekt, das die Bounding Box des Grundstücks enthält

    /**
     * Setzt das {@code AgriPVData}-Objekt für dieses Panel.
     * Dieses Objekt enthält wichtige Informationen wie die Bounding Box des Grundstücks,
     * die für die Skalierung der Geometrien auf dem Panel verwendet wird.
     * @param data Das {@code AgriPVData}-Objekt.
     */
    public void setAgriPVData(AgriPVData data) {
        this.data = data;
        repaint(); // Panel neu zeichnen, um die neuen Daten zu berücksichtigen
    }

    /**
     * Fügt eine Geometrie hinzu, die als Umriss gezeichnet werden soll.
     * Diese Methode wird typischerweise für Grundstücksgrenzen oder Modulumrisse verwendet.
     * @param geometry Die JTS-Geometrie, die hinzugefügt werden soll.
     * @param color Die Farbe für den Umriss der Geometrie.
     */
    public void addGeometry(Geometry geometry, Color color) {
        this.outlineGeometries.add(geometry);
        this.outlineColors.add(color);
        repaint(); // Panel neu zeichnen, um die neue Geometrie anzuzeigen
    }
    
    /**
     * Fügt eine Geometrie hinzu, die als gefüllte Fläche gezeichnet werden soll.
     * Diese Methode wird primär für die Heatmap der Verschattungswerte verwendet,
     * wobei jede Gitternetz-Zelle mit einer Farbe gefüllt wird.
     * @param geometry Die JTS-Geometrie (erwartet wird ein Polygon).
     * @param color Die Füllfarbe für die Geometrie.
     */
    public void addFilledGeometry(Geometry geometry, Color color) {
        this.filledGeometries.add(geometry);
        this.filledColors.add(color);
        // Kein repaint hier, da diese Methode oft in einer Schleife aufgerufen wird.
        // Das repaint sollte nach dem Hinzufügen aller Elemente erfolgen, um Performance zu sparen.
    }
    
    /**
     * Löscht alle derzeit auf dem Panel gespeicherten Geometrien (sowohl Umrisse als auch gefüllte Flächen).
     * Setzt das Panel in einen leeren Zustand zurück.
     */
    public void clearAll(){
        outlineGeometries.clear(); // Löscht alle Umrissgeometrien
        outlineColors.clear();     // Löscht alle Umrissfarben
        clearShadingGeometries();  // Löscht auch alle gefüllten Geometrien (Verschattungsschicht)
        repaint();                 // Panel neu zeichnen, um den leeren Zustand anzuzeigen
    }
    
    /**
     * Löscht nur die gefüllten Geometrien (die Verschattungsschicht).
     * Die Umrissgeometrien (z.B. Grundstücksgrenze, PV-Module) bleiben erhalten,
     * da sie oft als Basiskarte dienen.
     */
    public void clearShadingGeometries() {
        filledGeometries.clear(); // Löscht alle gefüllten Geometrien
        filledColors.clear();     // Löscht alle Füllfarben
        // Kein repaint hier, da diese Methode oft vor dem Hinzufügen neuer gefüllter Elemente erfolgt.
        // Das repaint sollte nach dem Hinzufügen der neuen Elemente erfolgen.
    }

    /**
     * Die {@code paintComponent}-Methode ist verantwortlich für das Zeichnen
     * der Geometrien auf dem Panel. Sie wird vom Swing-Framework aufgerufen,
     * wenn das Panel neu gezeichnet werden muss (z.B. bei {@code repaint()}-Aufrufen).
     * Zuerst werden die gefüllten Geometrien (Heatmap) gezeichnet, dann die Umrisse
     * (Grundstück, Module), um sicherzustellen, dass die Umrisse sichtbar sind.
     *
     * @param g Das {@code Graphics}-Objekt, das zum Zeichnen verwendet wird.
     */
    @Override
    protected void paintComponent(Graphics g) {
        super.paintComponent(g); // Ruft die paintComponent-Methode der Superklasse auf (löscht den Hintergrund)
        if (data == null) return; // Wenn keine Daten gesetzt sind, nichts zeichnen

        Graphics2D g2 = (Graphics2D) g; // Cast zu Graphics2D für erweiterte Zeichenfunktionen
        
        // Zuerst gefüllte Geometrien zeichnen (Hintergrundschicht für die Heatmap)
        for (int i = 0; i < filledGeometries.size(); i++) {
            drawGeometry(g2, filledGeometries.get(i), filledColors.get(i), data, getWidth(), getHeight(), true); // true für Füllung
        }

        // Dann Umrissgeometrien zeichnen (Vordergrundschicht, z.B. Grundstücksgrenze, Module)
        for (int i = 0; i < outlineGeometries.size(); i++) {
            drawGeometry(g2, outlineGeometries.get(i), outlineColors.get(i), data, getWidth(), getHeight(), false); // false für Umriss
        }
    }
    
    /**
     * Zeichnet eine einzelne JTS-Geometrie auf dem Panel.
     * Die Geometrien werden von ihren geografischen Koordinaten in Bildschirmkoordinaten
     * umgerechnet und skaliert, um sie an die Größe des Panels anzupassen.
     * Die Y-Achse wird umgedreht, da in geografischen Koordinaten Nord oft positive Y
     * bedeutet, während in Bildschirmkoordinaten positive Y nach unten geht.
     *
     * @param g Das {@code Graphics2D}-Objekt zum Zeichnen.
     * @param geometry Die zu zeichnende JTS-Geometrie (Polygon oder Point).
     * @param color Die Farbe, mit der die Geometrie gezeichnet werden soll.
     * @param data Das {@code AgriPVData}-Objekt, das die Bounding Box des Grundstücks zur Skalierung enthält.
     * @param panelWidth Die aktuelle Breite des Panels.
     * @param panelHeight Die aktuelle Höhe des Panels.
     * @param fill {@code true}, wenn das Polygon gefüllt werden soll; {@code false} für nur den Umriss.
     */
    protected void drawGeometry(Graphics2D g, Geometry geometry, Color color, AgriPVData data, int panelWidth, int panelHeight, boolean fill) {
        // Berechnet die Skalierungsfaktoren für Nord- und Ost-Dimensionen.
        // Ein Rand von 10 Pixeln wird abgezogen, um einen kleinen Puffer am Rand des Panels zu lassen.
        double scaleN = ((panelHeight - 10.0) / (data.maxn - data.minn));
        double scaleE = ((panelWidth - 10.0) / (data.maxe - data.mine));
        
        // Wählt den kleineren der beiden Skalierungsfaktoren, um sicherzustellen, dass die gesamte Geometrie
        // sichtbar ist und proportional skaliert wird (verhindert Verzerrungen).
        if (scaleN < scaleE) {
            scaleE = scaleN;
        }
        
        g.setColor(color); // Setzt die Zeichenfarbe

        // Verarbeitet Polygone
        if (geometry instanceof Polygon) {
            Polygon poly = (Polygon) geometry;
            // Holt die Koordinaten des äußeren Rings des Polygons
            Coordinate[] coords = poly.getExteriorRing().getCoordinates();

            // Arrays für die Bildschirmkoordinaten (X und Y)
            int[] x = new int[coords.length];
            int[] y = new int[coords.length];

            // Konvertiert jede geografische Koordinate in eine Bildschirmkoordinate
            for (int i = 0; i < coords.length; i++) {
                // Konvertierung der X-Koordinate:
                // Zuerst wird der geografische Mittelpunkt der Ost-Spannweite (data.mine + data.maxe) / 2.0
                // vom aktuellen X-Koordinate (coords[i].x) abgezogen. Das Ergebnis ist eine relative X-Position.
                // Diese relative X-Position wird dann mit dem Skalierungsfaktor multipliziert.
                // Schließlich wird der Mittelpunkt des Panels (panelWidth / 2.0) addiert, um die Position
                // relativ zum Panel-Mittelpunkt zu erhalten.
                x[i] = (int) ((panelWidth / 2.0) + (coords[i].x - (data.mine + data.maxe) / 2.0) * scaleE);
                
                // Konvertierung der Y-Koordinate:
                // Ähnlich wie bei X wird eine relative Y-Position berechnet.
                // Der Unterschied ist, dass hier die Y-Achse umgedreht wird (Subtraktion vom Panel-Mittelpunkt),
                // da in JTS positive Y nach Norden (oben) geht, während in Swing-Grafiken positive Y nach unten geht.
                y[i] = (int) ((panelHeight / 2.0) - (coords[i].y - (data.minn + data.maxn) / 2.0) * scaleE); 
            }

            // Zeichnet das Polygon entweder gefüllt oder als Umriss
            if (fill) {
                g.fillPolygon(x, y, coords.length); // Füllt das Polygon
            } else {
                g.drawPolygon(x, y, coords.length); // Zeichnet nur den Umriss des Polygons
            }
        }
        // Verarbeitet Punkte
        else if (geometry instanceof Point) {
            Coordinate coord = geometry.getCoordinate(); // Holt die Koordinate des Punkts
            // Konvertiert die geografische Koordinate des Punkts in Bildschirmkoordinaten
            int px = (int) ((panelWidth / 2.0) + (coord.x - (data.mine + data.maxe) / 2.0) * scaleE);
            int py = (int) ((panelHeight / 2.0) - (coord.y - (data.minn + data.maxn) / 2.0) * scaleE);
            // Zeichnet den Punkt als kleinen gefüllten Kreis
            g.fillOval(px - 2, py - 2, 4, 4); // Kreismittelpunkt (px,py), Breite/Höhe 4
        }
    }
}
