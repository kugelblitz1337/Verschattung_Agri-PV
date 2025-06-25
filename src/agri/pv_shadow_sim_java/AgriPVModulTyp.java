/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package agri.pv_shadow_sim_java;

/**
 *
 * @author roesc
 */
public class AgriPVModulTyp {
    
    private double PVPlattenLange; // In Metern Entlang der Schwenkmittelachse
    private double PVPlattenBreite;
    private double lndwrtsNchtNtzLange; // In Metern Entlang der Schwenkmittelachse
    private double lndwrtsNchtNtzBreite;
    private double hoheZuPlattenMitte; // In Metern, Boden zu Schwenkpunkt bzw Plattenmitte
    private int transparenz; // in %
    private int reflexion; // in %
    private double minBodenabstand; // In Metern
    private int neigung; // Parallel zu Boden = 0°
    private boolean schwenkbar;
    private String bezeichnung;
//    private int winkelbegrenzungen;
//    private int schwenkgeschwindigkeit;
    
    public AgriPVModulTyp(double PVPlattenLange, double PVPlattenBreite, double lndwrtsNchtNtzLange, double lndwrtsNchtNtzBreite, double hoheZuPlattenMitte, int transparenz, int reflexion, double minBodenabstand, int neigung, boolean schwenkbar, String bezeichnung){
        this.PVPlattenLange = PVPlattenLange;
        this.PVPlattenBreite = PVPlattenBreite;
        this.lndwrtsNchtNtzLange = lndwrtsNchtNtzLange;
        this.lndwrtsNchtNtzBreite = lndwrtsNchtNtzBreite;
        this.hoheZuPlattenMitte = hoheZuPlattenMitte;
        this.transparenz = transparenz;
        this.reflexion = reflexion;
        this.minBodenabstand = minBodenabstand;
        this.neigung = neigung;
        this.schwenkbar = schwenkbar;
        this.bezeichnung = bezeichnung;
    }
    
    public AgriPVModulTyp(double PVPlattenLange, double PVPlattenBreite, double lndwrtsNchtNtzLange, double lndwrtsNchtNtzBreite, double hoheZuPlattenMitte, double minBodenabstand, int neigung, boolean schwenkbar, String bezeichnung){
        this(PVPlattenLange, PVPlattenBreite, lndwrtsNchtNtzLange, lndwrtsNchtNtzBreite, hoheZuPlattenMitte, 0, 0, minBodenabstand, neigung, schwenkbar, bezeichnung);
    }
    
    public AgriPVModulTyp(double PVPlattenLange, double PVPlattenBreite, double lndwrtsNchtNtzLange, double lndwrtsNchtNtzBreite, double hoheZuPlattenMitte, int neigung, boolean schwenkbar, String bezeichnung){
        this(PVPlattenLange, PVPlattenBreite, lndwrtsNchtNtzLange, lndwrtsNchtNtzBreite, hoheZuPlattenMitte, 0, 0, 0, neigung, schwenkbar, bezeichnung);
    }

    public double getPVPlattenLange() {
        return PVPlattenLange;
    }

    public double getPVPlattenBreite() {
        return PVPlattenBreite;
    }

    public double getLndwrtsNchtNtzLange() {
        return lndwrtsNchtNtzLange;
    }

    public double getLndwrtsNchtNtzBreite() {
        return lndwrtsNchtNtzBreite;
    }
    
    public double getHoheZuPlattenMitte() {
        return hoheZuPlattenMitte;
    }

    public int getTransparenz() {
        return transparenz;
    }

    public int getReflexion() {
        return reflexion;
    }

    public double getMinBodenabstand() {
        return minBodenabstand;
    }

    public int getNeigung() {
        return neigung;
    }

    public boolean isSchwenkbar() {
        return schwenkbar;
    }

    public String getBezeichnung() {
        return bezeichnung;
    }
    
    @Override
    public String toString(){
        return bezeichnung;
    }
}
