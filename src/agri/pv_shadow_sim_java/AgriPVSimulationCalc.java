/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package agri.pv_shadow_sim_java;

import java.time.LocalDate;
import java.time.ZoneId;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import java.util.concurrent.Callable;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import javax.swing.SwingWorker;

import net.e175.klaus.solarpositioning.DeltaT;
import net.e175.klaus.solarpositioning.SPA;
import net.e175.klaus.solarpositioning.SolarPosition;

import org.locationtech.jts.geom.Coordinate;
import org.locationtech.jts.geom.GeometryFactory;
import org.locationtech.jts.geom.Polygon;
import org.locationtech.jts.algorithm.ConvexHull;
import org.locationtech.jts.geom.Geometry;
import org.locationtech.jts.geom.MultiPolygon;

/**
 * Die Klasse {@code AgriPVSimulationCalc} enthält die Kernlogik für die
 * Agri-PV-Verschattungssimulation. Sie ist verantwortlich für die Berechnung
 * der 3D-Geometrie von PV-Modulen, die Projektion ihrer Schatten auf den Boden,
 * die Aggregation dieser Schatten über die Zeit in einem Gitternetz und
 * die Vorbereitung der Daten für die Visualisierung.
 *
 * @author roesc
 */
public class AgriPVSimulationCalc {
    /**
     * Hauptmethode zum Testen der Funktionalität der AgriPVSimulationCalc-Klasse.
     * Führt eine Beispiel-Schattenberechnung für ein einzelnes Modul durch.
     *
     * @param args Kommandozeilenargumente (nicht verwendet).
     */
    public static void main(String[] args) {
        Test();
    }

    /**
     * Berechnet die 3D-Eckpunkte eines PV-Moduls basierend auf seinen Eigenschaften,
     * dem Bodenmittelpunkt und der Ausrichtung.
     * <p>
     * Das Modul wird als eine um seine Längsachse geneigte rechteckige Platte angenommen.
     * Die Längsachse ist horizontal und in Richtung des pvAzimuth ausgerichtet.
     * Der Neigungswinkel ist relativ zur Horizontalen.
     *
     * @param apmt Der {@code AgriPVModulTyp} mit den physikalischen Abmessungen und der Neigung.
     * @param midPoint Der Mittelpunkt des Moduls auf dem Boden (X, Y).
     * @param pvAzimuth Die Ausrichtung des Moduls in Grad von Norden im Uhrzeigersinn (0° ist Nord, 90° ist Ost).
     * @param solarAzimuthDegrees Der Azimutwinkel der Sonne in Grad (im Uhrzeigersinn von Norden).
     * @param solarZenithAngleDegrees Der Zenitwinkel der Sonne in Grad (von der Vertikalen).
     * @return Eine {@code ArrayList} von {@code Coordinate}-Objekten, die die 4 3D-Eckpunkte des PV-Moduls darstellen.
     * Jedes {@code Coordinate}-Objekt enthält X, Y und Z-Koordinaten.
     */
    public static ArrayList<Coordinate> getModule3DCorners(AgriPVModulTyp apmt, Coordinate midPoint, int pvAzimuth, double solarAzimuthDegrees, double solarZenithAngleDegrees) {
        ArrayList<Coordinate> corners3D = new ArrayList<>(); // Liste zur Speicherung der 3D-Eckpunkte

        double L = apmt.getPVPlattenLange(); // Tatsächliche Länge der PV-Platte
        double B = apmt.getPVPlattenBreite(); // Tatsächliche Breite/Höhe der PV-Platte
        // Höhe der Plattenmitte über dem Boden. Annahme: midPoint.z ist die Höhe des Bodens an dieser Stelle.
        // Wenn midPoint.z Double.NaN ist, wird 0.0 verwendet.
        double cz = (Double.isNaN(midPoint.z) ? 0.0 : midPoint.z) + apmt.getHoheZuPlattenMitte();
        int neigungDegrees = apmt.getNeigung(); // Neigungswinkel von der Horizontalen (0=flach, 90=vertikal)

        double cx = midPoint.x; // X-Koordinate des Modulmittelpunkts
        double cy = midPoint.y; // Y-Koordinate des Modulmittelpunkts

        double half_L = L / 2.0; // Halbe Modullänge
        double half_B = B / 2.0; // Halbe Modulbreite

        double tiltRad = Math.toRadians(neigungDegrees); // Neigungswinkel in Radiant
        // Azimut des PV-Moduls in Radiant. Subtraktion von PI, um Nord als 0 und Ost als PI/2 zu verwenden,
        double azimuthRad = Math.toRadians(pvAzimuth) - Math.PI; 
        
        // Überprüft, ob das PV-Modul fest installiert oder schwenkbar ist.
        // Aktuell ist nur die Berechnung für feste Module implementiert.
        if (!apmt.isSchwenkbar()) { // Fall 1: PV Modul nicht schwenkbar (feste Neigung)
            // Horizontale Verschiebung entlang der Modullänge (entlang pvAzimuth)
            // Die X-Komponente (Ost) wird durch cos(azimuthRad) und die Y-Komponente (Nord) durch sin(azimuthRad) bestimmt.
            double deltaX_L = half_L * Math.cos(azimuthRad);
            double deltaY_L = half_L * Math.sin(azimuthRad);

            // Horizontale Verschiebung der geneigten Modulbreite.
            // Die horizontale Projektion der Breite ist B * cos(tiltRad).
            // Diese Komponente ist senkrecht zur Längsachse des Moduls, daher wird der Azimut um 90 Grad verschoben.
            // Um hier aber in positiven Werten zu bleiben wird sin und cos vertauscht
            // Daher für die Breite senkrecht zur Längsachse:
            double deltaX_B_horiz = half_B * Math.cos(tiltRad) * Math.sin(azimuthRad); // X-Komponente der Breite
            double deltaY_B_horiz = half_B * Math.cos(tiltRad) * Math.cos(azimuthRad); // Y-Komponente der Breite
            
            // Vertikale Verschiebung durch die geneigte Modulbreite.
            // Die vertikale Komponente der Breite ist B * sin(tiltRad).
            // Dies ist die Z-Verschiebung vom Plattenmittelpunkt nach oben und unten.
            double deltaZ_B_vert = half_B * Math.sin(tiltRad);

            // Berechnung der 4 Eckpunkte des physischen Moduls in globalen 3D-Koordinaten.
            // Die Punkte werden relativ zum Mittelpunkt (cx, cy, cz) verschoben.
            
            // Punkt 1: Unten-Links (Relativ zur Modul-Längsachse und -Querachse)
            corners3D.add(new Coordinate(
                cx - deltaX_L - deltaX_B_horiz,
                cy - deltaY_L - deltaY_B_horiz,
                cz - deltaZ_B_vert
            ));

            // Punkt 2: Unten-Rechts
            corners3D.add(new Coordinate(
                cx + deltaX_L - deltaX_B_horiz,
                cy + deltaY_L - deltaY_B_horiz,
                cz - deltaZ_B_vert
            ));
            
            // Punkt 3: Oben-Rechts
            corners3D.add(new Coordinate(
                cx + deltaX_L + deltaX_B_horiz,
                cy + deltaY_L + deltaY_B_horiz,
                cz + deltaZ_B_vert
            ));

            // Punkt 4: Oben-Links
            corners3D.add(new Coordinate(
                cx - deltaX_L + deltaX_B_horiz,
                cy - deltaY_L + deltaY_B_horiz,
                cz + deltaZ_B_vert
            ));

        } else { // Fall 2: PV Modul ist schwenkbar
            // TODO: Implementierung der 3D-Eckpunktberechnung für schwenkbares PV-Modul.
            // Für schwenkbare Module müsste die Neigung (tiltRad) und eventuell der Azimut
            // dynamisch an den Sonnenstand angepasst werden, um die optimale Ausrichtung zu simulieren.
            System.out.println("TODO: Implement 3D corner calculation for swivelable PV module.");
        }

        return corners3D; // Gibt die Liste der berechneten 3D-Eckpunkte zurück
    }


    /**
     * Berechnet das Schattenpolygon, das ein einzelnes rechteckiges PV-Modul
     * auf einer ebenen Fläche (Z=0) wirft.
     *
     * @param module3DCorners Eine {@code ArrayList} von {@code Coordinate}-Objekten, die die 3D-Eckpunkte des PV-Moduls darstellen.
     * @param solarAzimuthDegrees Der Azimutwinkel der Sonne in Grad (im Uhrzeigersinn von Norden, 0° ist Nord).
     * @param solarZenithAngleDegrees Der Zenitwinkel der Sonne in Grad (von der Vertikalen, 0° ist direkt über Kopf, 90° ist am Horizont).
     * @return Ein JTS {@code Polygon}, das den Schatten darstellt, oder {@code null}, wenn die Sonne unter dem Horizont ist oder keine gültigen Eckpunkte vorliegen.
     */
    public static Polygon calculateShadowPolygon(ArrayList<Coordinate> module3DCorners, double solarAzimuthDegrees, double solarZenithAngleDegrees) {

        // Wenn der Zenitwinkel 90 Grad oder größer ist, ist die Sonne unter dem Horizont.
        // In diesem Fall gibt es keinen Schatten, der auf den Boden projiziert wird.
        if (solarZenithAngleDegrees >= 90.0) {
            return null; 
        }
        
        // Prüft, ob gültige 3D-Modul-Eckpunkte für die Berechnung vorhanden sind.
        if (module3DCorners == null || module3DCorners.isEmpty()) {
            System.err.println("Keine 3D-Modul-Eckpunkte für die Schattenberechnung bereitgestellt.");
            return null;
        }

        // Winkel von Grad in Radiant umrechnen, da mathematische Funktionen Radian erwarten.
        double solarAzimuthRad = Math.toRadians(solarAzimuthDegrees); 
        double solarZenithRad = Math.toRadians(solarZenithAngleDegrees);

        // Den Tangens des Zenitwinkels berechnen. Er wird verwendet, um die horizontale Verschiebung
        // des Schattens pro Höheneinheit zu bestimmen.
        double tanZenith = Math.tan(solarZenithRad);

        ArrayList<Coordinate> projectedCoords = new ArrayList<>(); // Liste zur Speicherung der projizierten 2D-Punkte
        GeometryFactory gf = new GeometryFactory(); // JTS GeometryFactory zur Erstellung von Geometrien

        // Jeden 3D-Punkt des Moduls auf die Bodenfläche (Z=0) projizieren.
        for (Coordinate p3d : module3DCorners) {
            // Die Schattenprojektionsformel für einen Punkt (x, y, z) auf eine horizontale Ebene (z=0):
            // x_proj = x - z * tan(Zenitwinkel) * sin(Schatten-Azimutwinkel)
            // y_proj = y - z * tan(Zenitwinkel) * cos(Schatten-Azimutwinkel)
            // Hier ist 'p3d.z' die Höhe des Punktes über dem Boden.
            double shadowXOffset = p3d.z * tanZenith * Math.sin(solarAzimuthRad);
            double shadowYOffset = p3d.z * tanZenith * Math.cos(solarAzimuthRad);

            // Fügt den projizierten 2D-Punkt zur Liste hinzu.
            // Die Konvexität der Schattenfläche wird aus den projizierten Eckpunkten gebildet.
            // Reflextion bei (new Coordinate(p3d.x + shadowXOffset, p3d.y + shadowYOffset));
            projectedCoords.add(new Coordinate(p3d.x - shadowXOffset, p3d.y - shadowYOffset));
        }

        // Den konvexen Rumpf der projizierten Punkte berechnen.
        // Der Schatten eines konvexen Objekts auf eine Ebene ist der projizierte konvexe Rumpf
        // der relevanten Eckpunkte des Objekts.
        Coordinate[] projectedArray = projectedCoords.toArray(new Coordinate[0]);
        ConvexHull convexHull = new ConvexHull(projectedArray, gf); // Erstellt den konvexen Rumpf
        
        // Das Ergebnis des konvexen Rumpfes ist ein {@code Geometry}-Objekt, das ein Polygon,
        // eine Linie oder ein Punkt sein kann. Für einen Schatten erwarten wir ein Polygon.
        if (convexHull.getConvexHull() instanceof Polygon) {
            return (Polygon) convexHull.getConvexHull(); // Gibt das Schattenpolygon zurück
        } else {
            // Falls es kein Polygon ist (z.B. ein einzelner Punkt bei direktem Sonnenlicht von oben,
            // oder kollineare Punkte), wird null zurückgegeben. Für eine sinnvolle Schattenfläche
            // benötigen wir mindestens 3 nicht-kollineare Punkte.
            System.out.println("Der konvexe Rumpf der projizierten Schattenpunkte ist kein Polygon (z.B. Linie oder Punkt).");
            return null;
        }
    }

    /**
     * Berechnet die Verschattung für alle gegebenen PV-Modul-Standorte
     * über eine definierte Zeitspanne und mit einer bestimmten Intervallgenauigkeit.
     * Diese Methode iteriert durch die Zeit und für jeden Modulmittelpunkt,
     * berechnet die Sonnenposition und das resultierende Schattenpolygon.
     * Die akkumulierten Schattenminuten werden im {@code AgriPVData}-Objekt gespeichert.
     * Die Berechnung wird in einem Hintergrund-Thread durchgeführt und nutzt
     * einen Thread-Pool für die parallele Schattenberechnung pro Zeitintervall.
     *
     * @param monate Die Zeitspanne in Monaten, für die die Simulation durchgeführt werden soll.
     * @param strtmnt Der Startmonat (1 = Januar, 12 = Dezember).
     * @param intrvMin Die Intervallgenauigkeit in Minuten, in der die Sonnenposition und Schatten neu berechnet werden.
     * @param startYear Jahreszahl in der die Simulation starten soll.
     * @param data Das {@code AgriPVData}-Objekt, das die Modulstandorte (mitlpuktPV) enthält
     * sowie die Verschattungswerte (shadingValues) und Gitternetzpolygone (gridPolygons).
     * @param gttrNtzMeter Die Gitternetzauflösung in Metern für die spätere Visualisierung.
     * @param worker Eine Referenz zum {@code SwingWorker}, um den Fortschritt an das GUI zu publizieren.
     * @param apmt Der {@code AgriPVModulTyp} des zu simulierenden Moduls.
     * @param pvAzimuth Die Ausrichtung der PV-Module-Hauptfläche bzw. Oberseite in Grad von Norden im Uhrzeigersinn.
     * @param elevation Höhe des Grundstücks über der Normal Null in Metern.
     */
    public static void calculateAllShading(
            int monate, int strtmnt, int intrvMin, int startYear,
            AgriPVData data, double gttrNtzMeter, SwingWorker<?, Integer> worker,
            AgriPVModulTyp apmt, int pvAzimuth, double elevation) {

        // Überprüft, ob schwenkbare Module unterstützt werden (aktuell nicht implementiert).
        if (apmt.isSchwenkbar()) {
            System.out.println("TODO: Die Verschattungsberechnung für schwenkbare PV-Module ist noch nicht implementiert.");
            return;
        }
        
        System.out.println(System.nanoTime() + " calculateAllShading gestartet");

        // Standortparameter. Diese könnten später dynamisch aus Daten oder der Konfiguration stammen.
        ZoneId zoneId = ZoneId.of("Europe/Berlin"); // Zeitzone für die Sonnenpositionsberechnung
        double latitude = 47.6641; // Breitengrad (konstant für den Test)
        double longitude = 9.4383; // Längengrad (konstant für den Test)
        
        // Initialisiert das Gitternetz und setzt die Verschattungswerte einmalig zu Beginn der Simulation zurück.
        initializeGridAndShadingValues(data, gttrNtzMeter);

        // Erstellt einen Thread-Pool mit der Anzahl der verfügbaren Prozessoren.
        // Dies ermöglicht die parallele Berechnung der Schattenpolygone für alle Module
        // innerhalb eines einzelnen Zeitintervalls.
        ExecutorService executor = Executors.newFixedThreadPool(Runtime.getRuntime().availableProcessors());

        // Berechnet die Gesamtzahl der Zeitschritte für die Fortschrittsanzeige.
        int totalTimeSteps = calculateTotalTimeSteps(monate, intrvMin);
        
        // Gesamtanzahl der Module und Initialisierung des Fortschrittszählers
        int totalModules = data.mitlpuktPV != null ? data.mitlpuktPV.size() : 0;
        int currentModuleCount = 0;
        
        // Iteration durch jeden PV-Modul-Standort, für den Schatten berechnet werden soll.
        if (data.mitlpuktPV != null) { // Stellt sicher, dass die Liste der Modulmittelpunkte nicht null ist
            
            // Liste, um Callable-Aufgaben für die Schattenvisualisierung aller Module zu speichern.
            List<Callable<Boolean>> moduleShadowVisualizationTasks = new ArrayList<>();
            
            for (Coordinate moduleMidPoint : data.mitlpuktPV) {
                
                if(executor.isShutdown())break;
                
                currentModuleCount++;
                int progress =(int) (currentModuleCount * 9900.0 / totalModules);
                data.kGUI.setJprgrsbrRunning(progress); // Aktualisiert die Fortschrittsanzeige in der GUI

                // Setzen des Startzeitpunkts für die Simulation für das aktuelle Modul.
                // ZonedDateTime handhabt den Jahreswechsel und die Monatslängen automatisch.
                ZonedDateTime currentDateTime = ZonedDateTime.of(startYear, strtmnt, 1, 0, 0, 0, 0, zoneId);
                ZonedDateTime endDate = currentDateTime.plusMonths(monate); // Enddatum der Simulation
                LocalDate lastDate = ZonedDateTime.now().toLocalDate();
                
                // 3D-Eckpunkte des Moduls berechnen (bei fixem Modul, die Sonnenazimut/Zenit sind hier noch irrelevant)
                final ArrayList<Coordinate> fixedModule3DCorners = getModule3DCorners(apmt, moduleMidPoint, pvAzimuth, 0.0, 0.0);

                // Liste, um alle Schattenpolygone für die verschiedenen Zeitpunkte eines Moduls zu sammeln.
                // Diese Liste wird dann verwendet, um die Schattenverlaufsflächen zu berechnen.
                ArrayList<Polygon> shadowsForCurrentModulPos = new ArrayList<>();
                
                // Liste, um Callable-Aufgaben für die Schattenberechnung(calculateShadowPolygon) des aktuellen Moduls zu speichern.
                List<Callable<Polygon>> moduleShadowTasks = new ArrayList<>();
                
                // Schleife durch die Zeitintervalle für das aktuelle Modul.
                while (currentDateTime.isBefore(endDate)) {
                    // Berechnung der Sonnenposition für den aktuellen Zeitpunkt.
                    // TODO: Hier kann für calculateSolarPosition() in einer weiteren Arbeit Wetterdaten wie Luftdruck in hPa und Temperatur in °C verwendet werden.
                    double deltaT = DeltaT.estimate(currentDateTime.toLocalDate()); // Schätzung von Delta T
                    SolarPosition position = SPA.calculateSolarPosition(
                            currentDateTime,
                            latitude,
                            longitude,
                            elevation,
                            deltaT
                    );

                    final double solarAzimuth = position.azimuth(); // Azimut der Sonne (final für Callable)
                    final double solarZenith = position.zenithAngle(); // Zenitwinkel der Sonne (final für Callable)

                    if(solarZenith>90){
                        // Geht zum nächsten Zeitintervall über, wenn die Sonne unterm Horizont liegt.
                        currentDateTime = currentDateTime.plusMinutes(intrvMin);
                        continue;
                    }
                    
                    LocalDate currentDate = currentDateTime.toLocalDate();
                    if (currentDate.isEqual(lastDate) == false){
                        moduleShadowTasks.add(() -> {
                            return null;
                        });
                        lastDate = currentDate;
                    }
                    
                    // Wenn das Modul schwenkbar wäre, müssten hier die 3D-Eckpunkte
                    // basierend auf dem aktuellen Sonnenstand neu berechnet werden.
                    if (apmt.isSchwenkbar()) {
                        // Erstellt eine Callable-Aufgabe für die Schattenberechnung eines einzelnen Moduls.
                        // Jede Callable-Aufgabe wird in einem separaten Thread im Executor-Service ausgeführt.
                        moduleShadowTasks.add(() -> {
                            // 3D-Eckpunkte des Moduls berechnen (potenziell angepasst für schwenkbare Module).
                            // Da schwenkbare Module nicht implementiert sind, ist dieser Block aktuell nur ein Platzhalter.
                            // Hier würde man die Modulausrichtung optimieren und die 3D-Eckpunkte neu berechnen.
                            ArrayList<Coordinate> module3DCorners = getModule3DCorners(apmt, moduleMidPoint, pvAzimuth, solarAzimuth, solarZenith);
                            if (module3DCorners.isEmpty()) {
                                return null; // Gibt null zurück, wenn keine 3D-Eckpunkte bestimmt werden konnten.
                            }
                            // Berechnet und gibt das Schattenpolygon für das Modul zurück.
                            return calculateShadowPolygon(module3DCorners, solarAzimuth, solarZenith);
                        });
                        
                    } else {
                        // Erstellt eine Callable-Aufgabe für die Schattenberechnung eines einzelnen Moduls.
                        // Jede Callable-Aufgabe wird in einem separaten Thread im Executor-Service ausgeführt.
                        moduleShadowTasks.add(() -> {
                            return calculateShadowPolygon(fixedModule3DCorners, solarAzimuth, solarZenith);
                        });
                    }

                    // Geht zum nächsten Zeitintervall über.
                    currentDateTime = currentDateTime.plusMinutes(intrvMin);
                }
                
                // Prüfen ob die Simulation abgebrochen wurde
                if((data.isInterupted || worker.isCancelled()) && !executor.isShutdown()){
                    executor.shutdown(); // Fährt den Thread-Pool herunter (wartet auf Beendigung der ausstehenden Aufgaben)
                    try {
                        // Wartet maximal 60 Sekunden, bis alle Aufgaben beendet sind.
                        if (!executor.awaitTermination(60, java.util.concurrent.TimeUnit.SECONDS)) {
                            executor.shutdownNow(); // Erzwingt das Herunterfahren, wenn die Zeit abläuft
                            System.err.println("Thread-Pool wurde nicht innerhalb der Frist beendet.");
                        }
                    } catch (InterruptedException e) {
                        executor.shutdownNow();
                        Thread.currentThread().interrupt();
                        System.err.println("Thread-Pool-Beendigung unterbrochen.");
                    }
                    System.out.println("Simulation abgebrochen.");
                    worker.cancel(true);
                    return;
                }
                
                // Führt alle Modul-Schattenberechnungsaufgaben parallel aus und wartet auf deren Abschluss.
                List<Future<Polygon>> futures = null;
                try {
                    futures = executor.invokeAll(moduleShadowTasks); // Führt die Aufgaben aus und gibt Future-Objekte zurück
                } catch (InterruptedException e) {
                    System.err.println("Simulation unterbrochen während der parallelen Schattenberechnung: " + e.getMessage());
                    executor.shutdown();
                    Thread.currentThread().interrupt(); // Setzt das Interrupt-Flag zurück
                    break; // Beendet die Schleife
                }

                if (futures != null) {
                    // Sammelt die Ergebnisse (Schattenpolygone) von allen parallelen Aufgaben.
                    for (Future<Polygon> future : futures) {
                        try {
                            Polygon shadow = future.get(); // Holt das Ergebnis (blockiert, bis Ergebnis verfügbar ist)
                            shadowsForCurrentModulPos.add(shadow); // Fügt gültige Schattenpolygone hinzu
                        } catch (Exception e) {
                            System.err.println("Fehler beim Abrufen des parallelen Schattenberechnungsergebnisses für ein Modul: " + e.getMessage());
                            e.printStackTrace();
                        }
                    }
                }
                
                // Erstellt eine Callable-Aufgabe für die Schattenvisualisierung eines einzelnen Moduls.
                // Jede Callable-Aufgabe wird in einem separaten Thread im Executor-Service ausgeführt.
                if (!shadowsForCurrentModulPos.isEmpty()) {
                    moduleShadowVisualizationTasks.add(() -> {
                        // Übergabe aller Schattenpolygone für das aktuelle Modul an die Visualisierungsmethode.
                        // Dies wird nach der Schleife über die Zeitintervalle ausgeführt,
                        // da visualizeShadingOnGrid die Schattenverläufe zwischen diesen Polygonen betrachtet.
                        // Diese Methode muss den Zugriff auf 'data.gridFields' synchronisieren,
                        // da sie die gemeinsamen Daten aktualisiert.
                        return visualizeShadingOnGrid(data, shadowsForCurrentModulPos, intrvMin);
                    });
                }
                
                // Prüfen ob die Simulation abgebrochen wurde
                if((data.isInterupted || worker.isCancelled()) && !executor.isShutdown()){
                    executor.shutdown(); // Fährt den Thread-Pool herunter (wartet auf Beendigung der ausstehenden Aufgaben)
                    try {
                        // Wartet maximal 60 Sekunden, bis alle Aufgaben beendet sind.
                        if (!executor.awaitTermination(60, java.util.concurrent.TimeUnit.SECONDS)) {
                            executor.shutdownNow(); // Erzwingt das Herunterfahren, wenn die Zeit abläuft
                            System.err.println("Thread-Pool wurde nicht innerhalb der Frist beendet.");
                        }
                    } catch (InterruptedException e) {
                        executor.shutdownNow();
                        Thread.currentThread().interrupt();
                        System.err.println("Thread-Pool-Beendigung unterbrochen.");
                    }
                    System.out.println("Simulation abgebrochen.");
                    worker.cancel(true);
                    return;
                }
            }
            
            
            // Gesamtanzahl der Module und Initialisierung des Fortschrittszählers
            data.kGUI.setJprgrsbrRunning(300,"Simulation läuft: 3%"); // Aktualisiert die Fortschrittsanzeige in der GUI
            
            System.out.println(System.nanoTime() + " Schattenvisualizierungsaufgaben gestartet");
            
            // Führt alle Modul-Schattenvisualizierungsaufgaben parallel aus und wartet auf deren Abschluss.
            List<Future<Boolean>> futures = null;
            try {
                futures = executor.invokeAll(moduleShadowVisualizationTasks); // Führt die Aufgaben aus und gibt Future-Objekte zurück
            } catch (InterruptedException e) {
                System.err.println("Simulation unterbrochen während der parallelen Schattenvisualizierung: " + e.getMessage());
                executor.shutdown();
                Thread.currentThread().interrupt(); // Setzt das Interrupt-Flag zurück
            }

            if (futures != null) {
                // Sammelt die Ergebnisse (Schattenpolygone) von allen parallelen Aufgaben.
                for (Future<Boolean> future : futures) {
                    try {
                        Boolean success = future.get(); // Holt das Ergebnis (blockiert, bis Ergebnis verfügbar ist)
                        if(!success)System.err.println("FEHLER bei Schattenvisualizierung"); // Fügt gültige Schattenpolygone hinzu
                    } catch (Exception e) {
                        System.err.println("Fehler beim Abrufen des parallelen Schattenvisualizierung für ein Modul: " + e.getMessage());
                        e.printStackTrace();
                    }
                }
            }
        }
        
        //Prüft ob der ExecutorService noch nicht beendet wurde
        if(executor.isShutdown() == false){
            executor.shutdown(); // Fährt den Thread-Pool herunter (wartet auf Beendigung der ausstehenden Aufgaben)
            try {
                // Wartet maximal 60 Sekunden, bis alle Aufgaben beendet sind.
                if (!executor.awaitTermination(60, java.util.concurrent.TimeUnit.SECONDS)) {
                    executor.shutdownNow(); // Erzwingt das Herunterfahren, wenn die Zeit abläuft
                    System.err.println("Thread-Pool wurde nicht innerhalb der Frist beendet.");
                }
            } catch (InterruptedException e) {
                executor.shutdownNow();
                Thread.currentThread().interrupt();
                System.err.println("Thread-Pool-Beendigung unterbrochen.");
            }
        }
        
        System.out.println(System.nanoTime() + " Schattenberechnung für alle Module abgeschlossen.");
    }

    
    /**
     * Initialisiert das Gitternetz und setzt die Verschattungswerte im {@code AgriPVData}-Objekt.
     * Diese Methode wird einmalig zu Beginn der Simulation aufgerufen, um sicherzustellen,
     * dass das Gitternetz korrekt dimensioniert und die Verschattungswerte auf Null gesetzt sind.
     *
     * @param data Das {@code AgriPVData}-Objekt, das die Grundstücksgrenzen, Verschattungswerte
     * und Gitternetzpolygone enthält.
     * @param gttrNtzMeter Die Gitternetzauflösung in Metern.
     */
    private static void initializeGridAndShadingValues(AgriPVData data, double gttrNtzMeter) {

        // Berechnet die Dimensionen des Gitternetzes: Anzahl der Zellen in Ost- und Nordrichtung.
        // Math.ceil stellt sicher, dass der gesamte Bereich des Grundstücks abgedeckt wird,
        // auch wenn es nicht exakt durch die Gitternetzauflösung teilbar ist.
        int numCellsE = (int) Math.ceil((data.maxe - data.mine) / gttrNtzMeter); // Anzahl der Spalten (East)
        int numCellsN = (int) Math.ceil((data.maxn - data.minn) / gttrNtzMeter); // Anzahl der Zeilen (North)

        // Überprüft, ob das Gitternetz neu initialisiert werden muss.
        // Dies ist der Fall, wenn das Array null ist oder wenn sich die berechneten Dimensionen
        // (basierend auf Grundstück und Auflösung) von den aktuellen Array-Dimensionen unterscheiden.
        if (data.gridFields == null ||
            data.gridFields.length != numCellsN || data.gridFields[0].length != numCellsE) {
            
            // Initialisiert das 2D-Array für die AgriPVGridField-Objekte samt Verschattungswerte und Gitternetzpolygone.
            // Beachten Sie die Reihenfolge: [Zeilen (Norden)][Spalten (Osten)].
            data.gridFields = new AgriPVGridField[numCellsN][numCellsE];
            Polygon gridPolygon;
            boolean inPlot;

            // Erstellt die JTS-Polygone für jedes einzelne Gitternetzfeld.
            for (int row = 0; row < numCellsN; row++) { // Iteriert über die Zeilen (Norden)
                for (int col = 0; col < numCellsE; col++) { // Iteriert über die Spalten (Osten)
                    // Berechnet die minimalen und maximalen X- und Y-Koordinaten für die aktuelle Zelle.
                    double minX = data.mine + col * gttrNtzMeter;
                    double maxX = minX + gttrNtzMeter;
                    double minY = data.minn + row * gttrNtzMeter;
                    double maxY = minY + gttrNtzMeter;

                    // Definiert die Eckpunkte des Quadrat-Polygons für die Zelle.
                    Coordinate[] coords = new Coordinate[]{
                        new Coordinate(minX, minY), // Unterer linker Punkt
                        new Coordinate(maxX, minY), // Unterer rechter Punkt
                        new Coordinate(maxX, maxY), // Oberer rechter Punkt
                        new Coordinate(minX, maxY), // Oberer linker Punkt
                        new Coordinate(minX, minY)  // Schließt das Polygon
                    };
                    // Erstellt das Polygon und speichert es im Array.
                    gridPolygon = data.gf.createPolygon(coords);
                    inPlot = gridPolygon.intersects(data.plotPolygon);
                    
                    data.gridFields[row][col] = new AgriPVGridField(0.0, gridPolygon, inPlot);
                }
            }
            System.out.println(System.nanoTime() + " Gitternetz und Verschattungswerte initialisiert. Größe: " + numCellsE + "x" + numCellsN);
        } else {
            // Wenn das Gitternetz bereits initialisiert ist und die Größe passt,
            // werden nur die `shadingValue` auf 0 zurückgesetzt, um eine neue Berechnung zu starten,
            // während die `gridPolygon` beibehalten werden können.
            for (int row = 0; row < data.gridFields.length; row++) {
                for (int col = 0; col < data.gridFields[row].length; col++) {
                    data.gridFields[row][col].shadingValue = 0.0;
                }
            }
            System.out.println("Bestehendes Gitternetz erkannt und geprüft. Verschattungswerte werden zurückgesetzt.");
        }
    }
    
    /**
     * Berechnet die Schattenverläufe aus benachbarten Schattenpolygonen
     * und addiert den flächenmäßigen Anteil der Verschattung.
     * Akkumuliert diese Schattenminuten für die gegebene Liste von Schattenpolygonen
     * auf dem Gitternetz. Diese Methode wird für jedes Module aufgerufen,
     * um die Schattenbeiträge der Module zu den jeweiligen Gitternetzfeldern hinzuzufügen.
     * <p>
     * Der Zugriff auf das {@code data.gridFields}-Array wird synchronisiert,
     * um Threadsicherheit zu gewährleisten, falls diese Methode parallel aufgerufen würde.
     *
     * @param data Das {@code AgriPVData}-Objekt, das die Grundstücksgrenzen, Verschattungswerte
     * und Gitternetzpolygone enthält.
     * @param shadowPolygons Eine Liste von Schattenpolygonen, die für ein einzelnes Modul über die Zeit berechnet wurden.
     * @param intrvMin Das Zeitintervall in Minuten, für das diese Schattenpolygone gelten (wird zur Gewichtung der Verschattung verwendet).
     * @return Boolean Ob ein Visualizirungswert addiert wurde
     */
    private static Boolean visualizeShadingOnGrid(AgriPVData data, ArrayList<Polygon> shadowPolygons, int intrvMin) {
        Boolean ret = Boolean.FALSE;
        
        // Schattenverläufe berechnen und Verschattung in das Gitternetz eintragen.
        // Iteriert über Paare von benachbarten Schattenpolygonen, um den Schattenverlauf
        // über ein Zeitintervall (zwischen zwei aufeinanderfolgenden Sonnenstandsberechnungen) zu berücksichtigen.
        for (int i = 0; i < shadowPolygons.size() - 1; i++) {
            Polygon shadow1 = shadowPolygons.get(i); // Erstes Schattenpolygon
            Polygon shadow2 = shadowPolygons.get(i + 1); // Nächstes Schattenpolygon
            // Überspringt die Berechnung, wenn eines der Schattenpolygone null ist
            // (z.B. wenn die Sonne unter dem Horizont war).
            if (shadow1 == null || shadow2 == null) {
                continue; 
            }

            // Kombiniert die Koordinaten beider Polygone, um den konvexen Rumpf zu bilden.
            // Der konvexe Rumpf einer Menge von Punkten ist das kleinste konvexe Polygon,
            // das alle Punkte enthält. Dies bildet effektiv die "Schattenverlaufsfläche".
            ArrayList<Coordinate> combinedCoords = new ArrayList<>(Arrays.asList(shadow1.getCoordinates()));
            combinedCoords.addAll(Arrays.asList(shadow2.getCoordinates()));

            // Erzeugt die Schattenverlaufsfläche mittels ConvexHull.
            Geometry schattenVerlaufsFlaeche = null;
            if (!combinedCoords.isEmpty()) {
                 ConvexHull hull = new ConvexHull(combinedCoords.toArray(new Coordinate[0]), data.gf);
                 schattenVerlaufsFlaeche = hull.getConvexHull(); // Holt das resultierende Geometry-Objekt
            }
           
            // Verarbeitet die berechnete Schattenverlaufsfläche, wenn sie ein gültiges Polygon ist.
            if (schattenVerlaufsFlaeche != null && schattenVerlaufsFlaeche instanceof Polygon) {
                // Iteriert über jedes Feld im Gitternetz.
                for (int row = 0; row < data.gridFields.length; row++) {
                    for (int col = 0; col < data.gridFields[0].length; col++) {
                        
                        if(data.gridFields[row][col].inPlot == false)continue; // Überspringt Felder außerhalb des Grundstücks
                        
                        Polygon gridCell = data.gridFields[row][col].gridPolygon; // Das aktuelle Gitternetzfeld

                        // Prüft, ob die Gitternetz-Zelle die Schattenverlaufsfläche schneidet.
                        if (gridCell.intersects(schattenVerlaufsFlaeche)) {
                            // Berechnet den geometrischen Schnittbereich zwischen der Gitternetz-Zelle
                            // und der Schattenverlaufsfläche.
                            Geometry intersection = gridCell.intersection(schattenVerlaufsFlaeche);

                            // Wenn der Schnittbereich ein Polygon oder MultiPolygon ist (d.h. eine Fläche),
                            // wird der flächenmäßige Anteil der Verschattung berechnet.
                            if (intersection instanceof Polygon || intersection instanceof MultiPolygon) {
                                double intersectedArea = intersection.getArea(); // Fläche des Schnittbereichs
                                double sVFArea = schattenVerlaufsFlaeche.getArea(); // Fläche der Schattenverlaufsfläche

                                if (sVFArea > 0) { // Vermeidet Division durch Null
                                    // Berechnet den prozentualen Anteil der überdeckten Fläche
                                    double percentageCovered = intersectedArea / sVFArea;
                                    
                                    // Synchronisiert den Zugriff auf das gemeinsam genutzte `gridFields`-Array.
                                    // Dies ist entscheidend, da mehrere Threads gleichzeitig versuchen könnten,
                                    // Werte zu derselben Zelle hinzuzufügen.
                                    synchronized (data.gridFields[row][col]) {
                                        // Addiert den proportionalen Wert zur Verschattung des Feldes.
                                        // Der Wert repräsentiert hier "Schattenminuten" pro Quadratmeter im Feld.
                                        // `percentageCovered` gibt an, welcher Anteil der Gitternetz-Zelle
                                        // vom Schatten überdeckt ist. Multiplikation mit `intrvMin` gewichtet
                                        // dies mit der Dauer des Zeitintervalls.
                                        data.gridFields[row][col].shadingValue += percentageCovered * intrvMin;
                                    }
                                    ret = Boolean.TRUE;
                                }
                            }
                        }
                    }
                if(data.isInterupted)return null;
                }
            } else {
                System.out.println("Schattenverlaufsfläche konnte nicht als Polygon gebildet werden oder ist null.");
            }
        }
        int prog =(int) (data.kGUI.getJprgrsbrRunning() + (9700.0 / data.mitlpuktPV.size()));
        data.kGUI.setJprgrsbrRunning(prog,"Simulation läuft: "+ (int)(prog/100) +"%");
        // Debugging-Ausgabe (auskommentiert): Kann verwendet werden, um die berechneten shadingValues zu überprüfen.
        // for (int row = 0; row < data.shadingValues.length; row++) {
        //     System.out.println(Arrays.toString(data.shadingValues[row]));
        // }
        return ret;
    }

    /**
     * Berechnet die geschätzte Gesamtzahl der Zeitschritte für die Fortschrittsanzeige.
     * Annahme: Ein Monat hat durchschnittlich 30 Tage.
     *
     * @param monate Die Anzahl der Monate der Simulation.
     * @param intrvMin Die Intervallgenauigkeit in Minuten.
     * @return Die geschätzte Gesamtzahl der Zeitschritte.
     */
    private static int calculateTotalTimeSteps(int monate, int intrvMin) {
        // Berechnung der Gesamtzahl der Minuten im Simulationszeitraum
        // (Monate * Tage pro Monat * Stunden pro Tag * Minuten pro Stunde)
        int totalMinutes = monate * 30 * 24 * 60; 
        // Teilt die Gesamtminuten durch die Intervallgenauigkeit, um die Anzahl der Zeitschritte zu erhalten.
        return totalMinutes / intrvMin;
    }

    /**
     * Testmethode zur Demonstration der Schattenberechnung für ein einzelnes
     * Modul zu verschiedenen Tageszeiten.
     * Diese Methode ist für Entwicklungs- und Debugging-Zwecke vorgesehen.
     */
    protected static void Test() {
        double latitude = 47.6641;  // Breitengrad des Teststandorts
        double longitude = 9.4383;  // Längengrad des Teststandorts
        double elevation = 427;       // Höhe über dem Meeresspiegel in Metern
        double pressure = 1010;     // Luftdruck in hPa (für SPA-Berechnung)
        double temperature = 11;    // Temperatur in °C (für SPA-Berechnung)

        // Beispiel-Modulparameter basierend auf AgriPVModulTyp: "Fest vertikale PV (Kategorie II)"
        // Diese Werte stammen aus der AgriPVModulTyp-Initialisierung in KonfigurationGUI
        double PVPlattenLange = 4;
        double PVPlattenBreite = 2;
        double hoheZuPlattenMitte = 2;
        double minBodenabstand = 2; 
        int neigung = 33; // Neigung des Moduls (33 Grad, fest vertikal)
        boolean schwenkbar = false; // Das Modul ist nicht schwenkbar
        String bezeichnung = "Fest vertikale PV (Kategorie II)";
        
        // Erstellt eine Instanz des AgriPVModulTyp für den Test
        AgriPVModulTyp testModulTyp = new AgriPVModulTyp(
                PVPlattenLange, PVPlattenBreite, // tatsächliche Abmessungen
                2.1256, 0.3, // lndwrtsNchtNtzLange, lndwrtsNchtNtzBreite (Bodengrundriss, hier für diesen Test nicht relevant)
                hoheZuPlattenMitte,
                minBodenabstand,
                neigung,
                schwenkbar,
                bezeichnung
        );

        // Beispiel: Ein einzelnes Modul am Koordinatenpunkt (0.0, 0.0)
        Coordinate moduleMidPoint = new Coordinate(0.0, 0.0);
        int moduleAzimuth = 180; // Ausrichtung des Moduls: 180 Grad ist Süden

        System.out.println("Starte Schattenberechnungs-Test für 2025-06-21:");

        // Berechnet die 3D-Eckpunkte des Moduls. Da es ein festes Modul ist,
        // sind solarAzimuth und solarZenith für diese Berechnung nicht relevant.
        ArrayList<Coordinate> module3DCorners = getModule3DCorners(testModulTyp, moduleMidPoint, moduleAzimuth, 0.0, 0.0);

        // Gibt die berechneten 3D-Eckpunkte aus (zu Debugging-Zwecken)
        for (Coordinate cord : module3DCorners) {
            System.out.println("Cord: x(" + cord.x + "," + cord.y + "," + cord.z +")");
        }

        GeometryFactory gf = new GeometryFactory(); // JTS GeometryFactory
        ArrayList<Coordinate> allShadowPoints = new ArrayList<>(); // Sammelt alle Schattenpunkte für den Gesamtschattenverlauf

        // Iteriert durch typische Tageslichtstunden, um den Schattenverlauf zu simulieren
        for (int i = 6; i < 8; i++) { // Von 6:00 Uhr bis 7:00 Uhr
            // Erstellt ein ZonedDateTime-Objekt für den aktuellen Zeitpunkt
            ZonedDateTime dateTime = ZonedDateTime.of(2025, 6, 21, i, 0, 0, 0, ZoneId.of("Europe/Berlin"));
            double deltaT = DeltaT.estimate(dateTime.toLocalDate()); // Schätzung von ΔT (Differenz zwischen UT1 und UTC)
            
            // Berechnet die Sonnenposition (Azimut und Zenitwinkel) für den aktuellen Zeitpunkt
            SolarPosition position = SPA.calculateSolarPosition(
                    dateTime,
                    latitude,
                    longitude,
                    elevation,
                    deltaT,
                    pressure,
                    temperature
            );

            double solarAzimuth = position.azimuth(); // Azimutwinkel der Sonne
            double solarZenith = position.zenithAngle(); // Zenitwinkel der Sonne

            // Gibt die Sonnenposition für den aktuellen Zeitpunkt aus
            System.out.println("Uhrzeit: " + i + ":00h - Azimut: " + String.format("%.2f", solarAzimuth) + "°, Zenit: " + String.format("%.2f", solarZenith) + "°");

            // Berechnet das Schattenpolygon für das Modul und den aktuellen Sonnenstand
            Polygon shadowPolygon = calculateShadowPolygon(
                    module3DCorners,
                    solarAzimuth,
                    solarZenith
            );

            // Verarbeitet das berechnete Schattenpolygon
            if (shadowPolygon != null) {
                System.out.println("  Schatten-Polygon: " + shadowPolygon.toText()); // Gibt das WKT des Schattenpolygons aus
                // Fügt die Koordinaten des aktuellen Schattenpolygons zur Liste hinzu,
                // um später den gesamten konvexen Rumpf des Schattenverlaufs zu berechnen.
                for (Coordinate co : shadowPolygon.getCoordinates()) {
                    allShadowPoints.add(co);
                }
            } else {
                System.out.println("  Kein Schatten auf dem Boden (Sonne unter Horizont oder andere Probleme).");
            }
        }
        
        // Berechnet den konvexen Rumpf aller gesammelten Schattenpunkte.
        // Dies ergibt eine Fläche, die den gesamten über den Zeitraum geworfenen Schatten umschließt.
        Coordinate[] allShadowPointsArray = allShadowPoints.toArray(new Coordinate[0]);
        Geometry hull = new ConvexHull(allShadowPointsArray, gf).getConvexHull();

        // Gibt die Koordinaten des resultierenden konvexen Rumpfes aus
        System.out.println("\nKonvexer Rumpf des gesamten Schattenverlaufs:");
        for (Coordinate co : hull.getCoordinates()) {
            System.out.println("(" + co.x + ", " + co.y + ")");
        }
    }
}
