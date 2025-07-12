/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package agri.pv_shadow_sim_java;

import java.io.Serializable;
import java.util.ArrayList;
import org.locationtech.jts.geom.*;

/**
 * Die Klasse {@code AgriPVData} dient als zentraler Datenspeicher für alle
 * relevanten Informationen, die während der Agri-PV-Simulationsberechnungen
 * und -visualisierungen benötigt werden. Sie enthält Grundstücksdaten,
 * PV-Modul-Platzierungen und die berechneten Verschattungswerte.
 * Diese Klasse ist serialisierbar, um den Zustand der Simulation speichern
 * und laden zu können.
 *
 * @author roesc
 */
public class AgriPVData implements Serializable{
    
    /**
     * Der Name der Gemeinde des Flurstücks.
     */
    protected String gemeindename; 
    /**
     * Der Zähler der Flurstücksnummer.
     */
    protected int flurstueckszaehler; 
    /**
     * Der Nenner der Flurstücksnummer.
     */
    protected int flurstuecksnenner; 
    /**
     * Die amtliche Fläche des Flurstücks in Quadratmetern.
     */
    protected int amtFlaeche; 
    /**
     * Die Koordinaten der Grundstücksgrenze.
     * Das erste Array enthält die Ost-Koordinaten (East), das zweite die Nord-Koordinaten (North).
     * Format: {@code grundstuecksgrenze[0]=East, grundstuecksgrenze[1]=North}.
     */
    protected double[][] grundstuecksgrenze; 
    /**
     * Die Anzahl der Punkte, die die Grundstücksgrenze definieren.
     */
    protected int points; 
    /**
     * Die minimale Ost-Koordinate (Easting) der Bounding Box des Grundstücks.
     */
    protected double mine; 
    /**
     * Die minimale Nord-Koordinate (Northing) der Bounding Box des Grundstücks.
     */
    protected double minn; 
    /**
     * Die maximale Ost-Koordinate (Easting) der Bounding Box des Grundstücks.
     */
    protected double maxe; 
    /**
     * Die maximale Nord-Koordinate (Northing) der Bounding Box des Grundstücks.
     */
    protected double maxn; 
    
    /**
     * Eine Liste von {@code Coordinate}-Objekten, die die Mittelpunkte der platzierten PV-Module repräsentieren.
     */
    protected ArrayList<Coordinate> mitlpuktPV; 
    /**
     * Ein JTS-Polygon, das die Grundstücksgrenze repräsentiert.
     */
    protected Polygon plotPolygon; 
    
    /**
     * Ein 2D-Array, das {@code AgriPVGridField}-Objekte speichert. Jedes Objekt repräsentiert
     * ein Gitternetzfeld auf dem Grundstück und enthält dessen Verschattungswert und Polygon sowie einen {@code boolean inPlot}.
     */
    protected AgriPVGridField[][] gridFields; 
    /**
     * Eine JTS {@code GeometryFactory} zur Erstellung von Geometrien.
     */
    protected GeometryFactory gf = new GeometryFactory(); 
    
    /**
     * Eine Referenz zur {@code KonfigurationGUI}, um den Fortschritt und Status in der GUI zu aktualisieren.
     */
    protected KonfigurationGUI kGUI;
    
    /**
     * Ein Flag, das anzeigt, ob die Simulation unterbrochen wurde.
     * Wird auf {@code true} gesetzt, wenn der Benutzer die Simulation abbricht.
     */
    protected boolean isInterupted;
    
    /**
     * Konvertiert Grad, Minuten und Sekunden in einen Dezimalwert.
     * Diese Methode wird typischerweise für die Umrechnung von geografischen
     * Koordinaten (Längen- und Breitengrad) in ein Dezimalformat verwendet.
     *
     * @param grad Der Gradanteil der Koordinate.
     * @param minuten Der Minutenanteil der Koordinate.
     * @param sekunden Der Sekundenanteil der Koordinate (kann Dezimalstellen enthalten).
     * @return Der konvertierte Dezimalwert der Koordinate.
     */
    public static double dmsToDecimal(int grad, int minuten, double sekunden) {
        // Die Formel für die Umrechnung lautet: Grad + Minuten/60 + Sekunden/3600
        double dezimal = grad + minuten / 60.0 + sekunden / 3600.0;
        return dezimal;
    }
}
