package agri.pv_shadow_sim_java;

import java.io.Serializable;
import java.util.ArrayList;
import org.locationtech.jts.geom.*;

/**
 * Diese Klasse dient als Datenspeicher zwischen den Klassen und Methoden
 * @author roesc
 */
public class AgriPVData implements Serializable{
    
    protected String gemeindename;
    protected int flurstueckszaehler;
    protected int flurstuecksnenner;    
    protected double[][] grundstuecksgrenze;
    protected int points;
    protected double mine;
    protected double minn;
    protected double maxe;
    protected double maxn;
    
    protected ArrayList<Coordinate> mitlpuktPV;
    protected Polygon plotPolygon;
    
    protected double[][] shadingValues; // Speichert die kumulierten Verschattungswerte (z.B. Schattenminuten)
    protected Polygon[][] gridPolygons; // Speichert die JTS-Polygone für jedes Gitternetzfeld
    
    public static double dmsToDecimal(int grad, int minuten, double sekunden) {
        double dezimal = grad + minuten / 60.0 + sekunden / 3600.0;
        return dezimal;
    }
}
