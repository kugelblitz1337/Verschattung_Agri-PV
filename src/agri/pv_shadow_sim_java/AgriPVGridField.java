/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package agri.pv_shadow_sim_java;

import org.locationtech.jts.geom.Polygon;

/**
 *
 * @author roesc
 */
public class AgriPVGridField {
    
    protected double shadingValue; // Speichert die kumulierten Verschattungswerte (z.B. Schattenminuten) für jedes Gitternetzfeld
    protected Polygon gridPolygon; // Speichert die JTS-Polygone, die jedes Gitternetzfeld auf dem Grundstück repräsentieren
    protected boolean inPlot; // Speichert eine Boolean, der aussagt on ob das Feld innerhalb der Grundstücksgrenze liegt oder nicht
    
    public AgriPVGridField(double shadingValue, Polygon gridPolygon, boolean inPlot){
        this.shadingValue = shadingValue;
        this.gridPolygon = gridPolygon;
        this.inPlot = inPlot;
    }
    
    
}
