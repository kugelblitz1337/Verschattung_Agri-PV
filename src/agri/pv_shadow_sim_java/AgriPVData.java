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
    
    protected String gemeindename; // Name der Gemeinde des Flurstücks
    protected int flurstueckszaehler; // Zähler der Flurstücksnummer
    protected int flurstuecksnenner; // Nenner der Flurstücksnummer
    protected int amtFlaeche; // Amtliche Fläche des Flurstücks in Quadratmetern
    protected double[][] grundstuecksgrenze; // Koordinaten der Grundstücksgrenze [0]=East, [1]=North
    protected int points; // Anzahl der Punkte, die die Grundstücksgrenze definieren
    protected double mine; // Minimale Ost-Koordinate (Easting) des Grundstücks
    protected double minn; // Minimale Nord-Koordinate (Northing) des Grundstücks
    protected double maxe; // Maximale Ost-Koordinate (Easting) des Grundstücks
    protected double maxn; // Maximale Nord-Koordinate (Northing) des Grundstücks
    
    protected ArrayList<Coordinate> mitlpuktPV; // Liste der Mittelpunkte der platzierten PV-Module
    protected Polygon plotPolygon; // JTS-Polygon, das die Grundstücksgrenze repräsentiert
    
    protected AgriPVGridField[][] gridFields; // Speichert die AgriPVGridField-Objekte, die jedes Gitternetzfeld auf dem Grundstück und dessen Verschattungswert repräsentieren
    protected GeometryFactory gf = new GeometryFactory(); // JTS GeometryFactory zur Erstellung von Geometrien
    
    protected KonfigurationGUI kGUI;
    
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
