/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package agri.pv_shadow_sim_java;

import org.locationtech.jts.geom.Polygon;

/**
 * Die Klasse {@code AgriPVGridField} repräsentiert ein einzelnes Feld innerhalb
 * des Gitternetzes, das über dem Flurstück liegt. Es speichert den kumulierten
 * Verschattungswert, das geometrische Polygon des Feldes und einen Status,
 * ob das Feld innerhalb der Grundstücksgrenze liegt.
 *
 * @author roesc
 */
public class AgriPVGridField {
    
    
    /**
     * Speichert die kumulierten Verschattungswerte (z.B. Schattenminuten) für dieses Gitternetzfeld.
     */
    protected double shadingValue; 
    /**
     * Speichert das JTS-Polygon, das dieses Gitternetzfeld repräsentiert.
     */
    protected Polygon gridPolygon; 
    /**
     * Ein Boolean-Wert, der aussagt, ob das Feld innerhalb der Grundstücksgrenze liegt oder nicht.
     */
    protected boolean inPlot; 
    
    /**
     * Konstruktor für ein neues {@code AgriPVGridField}-Objekt.
     *
     * @param shadingValue Der initiale Verschattungswert für das Feld.
     * @param gridPolygon Das JTS-Polygon, das die Geometrie des Feldes definiert.
     * @param inPlot Ein Boolean-Wert, der angibt, ob das Feld innerhalb der Grundstücksgrenze liegt.
     */
    public AgriPVGridField(double shadingValue, Polygon gridPolygon, boolean inPlot){
        this.shadingValue = shadingValue;
        this.gridPolygon = gridPolygon;
        this.inPlot = inPlot;
    }
    
    
}
