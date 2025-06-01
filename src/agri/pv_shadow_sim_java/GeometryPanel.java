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
 *
 * @author roesc
 */
public class GeometryPanel extends JPanel {

    private final ArrayList<Geometry> geometries = new ArrayList<>();
    private final ArrayList<Color> colors = new ArrayList<>();
    private AgriPVData data;

    public void setAgriPVData(AgriPVData data) {
        this.data = data;
        repaint();
    }

    public void addGeometry(Geometry geometry, Color color) {
        this.geometries.add(geometry);
        this.colors.add(color);
        repaint();
    }
    
    public void clearAll(){
        geometries.clear();
        colors.clear();
        repaint();
    }

    @Override
    protected void paintComponent(Graphics g) {
        super.paintComponent(g);
        if (data == null) return;

        Graphics2D g2 = (Graphics2D) g;
        for (int i = 0; i < geometries.size(); i++) {
            drawGeometry(g2, geometries.get(i), colors.get(i), data, getWidth(), getHeight());
        }
    }
    
    protected void drawGeometry(Graphics2D g, Geometry geometry, Color color, AgriPVData data, int panelWidth, int panelHeight) {
        double scaleN = ((panelHeight - 10.0) / (data.maxn - data.minn));
        double scaleE = ((panelWidth - 10.0) / (data.maxe - data.mine));
        if (scaleN < scaleE) {
            scaleE = scaleN;
        }
        
        g.setColor(color);

        if (geometry instanceof Polygon) {
            Polygon poly = (Polygon) geometry;
            Coordinate[] coords = poly.getExteriorRing().getCoordinates();

            int[] x = new int[coords.length];
            int[] y = new int[coords.length];

            for (int i = 0; i < coords.length; i++) {
                x[i] = (int) ((panelWidth / 2.0) + (coords[i].x - (data.mine + data.maxe) / 2.0) * scaleE);
                y[i] = (int) ((panelHeight / 2.0) - (coords[i].y - (data.minn + data.maxn) / 2.0) * scaleE);
            }

            g.drawPolygon(x, y, coords.length);
//            g.fillPolygon(x, y, coords.length); // Optional: auskommentieren wenn nur Kontur
        }

        else if (geometry instanceof Point) {
            Coordinate coord = geometry.getCoordinate();
            int px = (int) ((panelWidth / 2.0) + (coord.x - (data.mine + data.maxe) / 2.0) * scaleE);
            int py = (int) ((panelHeight / 2.0) - (coord.y - (data.minn + data.maxn) / 2.0) * scaleE);
            g.fillOval(px - 2, py - 2, 4, 4);
        }
    }
}
