package agri.pv_shadow_sim_java;

import java.io.Serializable;
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
    
    protected int[][] mitlpuktPV;
    protected Polygon plotPolygon;
    
}
