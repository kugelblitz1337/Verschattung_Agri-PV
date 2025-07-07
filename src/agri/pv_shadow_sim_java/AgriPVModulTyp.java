/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package agri.pv_shadow_sim_java;

/**
 * Die Klasse {@code AgriPVModulTyp} repräsentiert einen Typ von Agri-PV-Modulen
 * mit all seinen relevanten physikalischen und betrieblichen Eigenschaften.
 * Sie speichert Details wie Abmessungen, Transparenz, Reflexion, Mindestbodenabstand,
 * Neigung, Schwenkbarkeit und eine allgemeine Bezeichnung.
 *
 * @author roesc
 */
public class AgriPVModulTyp {
    
    private double PVPlattenLange; // In Metern Entlang der Schwenk- bzw. Modulreihenachse
    private double PVPlattenBreite; // In Metern, senkrecht zur Schwenk- bzw. Modulreihenachse
    private double lndwrtsNchtNtzLange; // In Metern, landwirtschaftlich nicht nutzbare Länge des Bodengrundrisses
    private double lndwrtsNchtNtzBreite; // In Metern, landwirtschaftlich nicht nutzbare Breite des Bodengrundrisses
    private double hoheZuPlattenMitte; // In Metern, Höhe vom Boden zum Schwenkpunkt bzw. zur Plattenmitte
    private int transparenz; // In Prozent (0-100), Transparenz der PV-Platte
    private int reflexion; // In Prozent (0-100), Reflexionsgrad der PV-Platte
    private double minBodenabstand; // In Metern, Mindestabstand der PV-Platte zum Boden
    private int neigung; // In Grad (0-90), Neigungswinkel der Platte parallel zum Boden = 0°
    private boolean schwenkbar; // True, wenn das Modul schwenkbar ist, sonst False
    private String bezeichnung; // Eine beschreibende Bezeichnung für den Modultyp
//    private int winkelbegrenzungen; // Optionale Eigenschaft: Winkelbegrenzungen für schwenkbare Module
//    private int schwenkgeschwindigkeit; // Optionale Eigenschaft: Schwenkgeschwindigkeit für schwenkbare Module
    
    /**
     * Konstruktor für einen Agri-PV-Modultyp mit allen detaillierten Eigenschaften.
     *
     * @param PVPlattenLange Die Länge der PV-Platte in Metern (entlang der Schwenk- bzw. Modulreihenachse).
     * @param PVPlattenBreite Die Breite der PV-Platte in Metern (senkrecht zur Schwenk- bzw. Modulreihenachse).
     * @param lndwrtsNchtNtzLange Die landwirtschaftlich nicht nutzbare Länge des Bodengrundrisses in Metern.
     * @param lndwrtsNchtNtzBreite Die landwirtschaftlich nicht nutzbare Breite des Bodengrundrisses in Metern.
     * @param hoheZuPlattenMitte Die Höhe vom Boden zum Schwenkpunkt bzw. zur Plattenmitte in Metern.
     * @param transparenz Die Transparenz des Moduls in Prozent.
     * @param reflexion Der Reflexionsgrad des Moduls in Prozent.
     * @param minBodenabstand Der Mindestabstand des Moduls zum Boden in Metern.
     * @param neigung Der Neigungswinkel des Moduls in Grad (0° ist parallel zum Boden).
     * @param schwenkbar Gibt an, ob das Modul schwenkbar ist.
     * @param bezeichnung Eine beschreibende Bezeichnung für den Modultyp.
     */
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
    
    /**
     * Konstruktor für einen Agri-PV-Modultyp mit Standardwerten für Transparenz und Reflexion (0%).
     *
     * @param PVPlattenLange Die Länge der PV-Platte in Metern.
     * @param PVPlattenBreite Die Breite der PV-Platte in Metern.
     * @param lndwrtsNchtNtzLange Die landwirtschaftlich nicht nutzbare Länge des Bodengrundrisses in Metern.
     * @param lndwrtsNchtNtzBreite Die landwirtschaftlich nicht nutzbare Breite des Bodengrundrisses in Metern.
     * @param hoheZuPlattenMitte Die Höhe vom Boden zum Schwenkpunkt bzw. zur Plattenmitte in Metern.
     * @param minBodenabstand Der Mindestabstand des Moduls zum Boden in Metern.
     * @param neigung Der Neigungswinkel des Moduls in Grad.
     * @param schwenkbar Gibt an, ob das Modul schwenkbar ist.
     * @param bezeichnung Eine beschreibende Bezeichnung.
     */
    public AgriPVModulTyp(double PVPlattenLange, double PVPlattenBreite, double lndwrtsNchtNtzLange, double lndwrtsNchtNtzBreite, double hoheZuPlattenMitte, double minBodenabstand, int neigung, boolean schwenkbar, String bezeichnung){
        this(PVPlattenLange, PVPlattenBreite, lndwrtsNchtNtzLange, lndwrtsNchtNtzBreite, hoheZuPlattenMitte, 0, 0, minBodenabstand, neigung, schwenkbar, bezeichnung);
    }
    
    /**
     * Konstruktor für einen Agri-PV-Modultyp mit Standardwerten für Transparenz, Reflexion und Mindestbodenabstand (0%).
     *
     * @param PVPlattenLange Die Länge der PV-Platte in Metern.
     * @param PVPlattenBreite Die Breite der PV-Platte in Metern.
     * @param lndwrtsNchtNtzLange Die landwirtschaftlich nicht nutzbare Länge des Bodengrundrisses in Metern.
     * @param lndwrtsNchtNtzBreite Die landwirtschaftlich nicht nutzbare Breite des Bodengrundrisses in Metern.
     * @param hoheZuPlattenMitte Die Höhe vom Boden zum Schwenkpunkt bzw. zur Plattenmitte in Metern.
     * @param neigung Der Neigungswinkel des Moduls in Grad.
     * @param schwenkbar Gibt an, ob das Modul schwenkbar ist.
     * @param bezeichnung Eine beschreibende Bezeichnung.
     */
    public AgriPVModulTyp(double PVPlattenLange, double PVPlattenBreite, double lndwrtsNchtNtzLange, double lndwrtsNchtNtzBreite, double hoheZuPlattenMitte, int neigung, boolean schwenkbar, String bezeichnung){
        this(PVPlattenLange, PVPlattenBreite, lndwrtsNchtNtzLange, lndwrtsNchtNtzBreite, hoheZuPlattenMitte, 0, 0, 0, neigung, schwenkbar, bezeichnung);
    }

    /**
     * Gibt die Länge der PV-Platte zurück.
     * @return Die Länge der PV-Platte in Metern.
     */
    public double getPVPlattenLange() {
        return PVPlattenLange;
    }

    /**
     * Gibt die Breite der PV-Platte zurück.
     * @return Die Breite der PV-Platte in Metern.
     */
    public double getPVPlattenBreite() {
        return PVPlattenBreite;
    }

    /**
     * Gibt die landwirtschaftlich nicht nutzbare Länge des Bodengrundrisses zurück.
     * @return Die landwirtschaftlich nicht nutzbare Länge in Metern.
     */
    public double getLndwrtsNchtNtzLange() {
        return lndwrtsNchtNtzLange;
    }

    /**
     * Gibt die landwirtschaftlich nicht nutzbare Breite des Bodengrundrisses zurück.
     * @return Die landwirtschaftlich nicht nutzbare Breite in Metern.
     */
    public double getLndwrtsNchtNtzBreite() {
        return lndwrtsNchtNtzBreite;
    }
    
    /**
     * Gibt die Höhe vom Boden zur Plattenmitte zurück.
     * @return Die Höhe in Metern.
     */
    public double getHoheZuPlattenMitte() {
        return hoheZuPlattenMitte;
    }

    /**
     * Gibt die Transparenz des Moduls zurück.
     * @return Die Transparenz in Prozent.
     */
    public int getTransparenz() {
        return transparenz;
    }

    /**
     * Gibt den Reflexionsgrad des Moduls zurück.
     * @return Der Reflexionsgrad in Prozent.
     */
    public int getReflexion() {
        return reflexion;
    }

    /**
     * Gibt den Mindestabstand des Moduls zum Boden zurück.
     * @return Der Mindestbodenabstand in Metern.
     */
    public double getMinBodenabstand() {
        return minBodenabstand;
    }

    /**
     * Gibt den Neigungswinkel des Moduls zurück.
     * @return Der Neigungswinkel in Grad.
     */
    public int getNeigung() {
        return neigung;
    }

    /**
     * Prüft, ob das Modul schwenkbar ist.
     * @return True, wenn schwenkbar, sonst False.
     */
    public boolean isSchwenkbar() {
        return schwenkbar;
    }

    /**
     * Gibt die Bezeichnung des Modultyps zurück.
     * @return Die Bezeichnung als String.
     */
    public String getBezeichnung() {
        return bezeichnung;
    }
    
    /**
     * Gibt die Bezeichnung des Modultyps als String-Repräsentation zurück.
     * Diese Methode wird verwendet, um den Modultyp in JComboBoxen oder ähnlichen
     * UI-Elementen darzustellen.
     * @return Die Bezeichnung des Modultyps.
     */
    @Override
    public String toString(){
        return bezeichnung;
    }
}
