/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package agri.pv_shadow_sim_java;

import java.util.ArrayList;
import org.apache.http.HttpResponse;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.util.EntityUtils;
import org.json.JSONArray;
import org.json.JSONObject;

/**
 * Die Klasse {@code ALKISApiClient} stellt Methoden zur Verfügung, um mit einer
 * externen ALKIS-API zu interagieren. Sie ermöglicht das Anmelden (Login),
 * das Abrufen von Gemarkungen und das Suchen nach Flurstücken
 * mittels Zähler/Nenner oder Koordinaten. Die Kommunikation erfolgt über HTTP
 * mit JSON-Daten und JWT-Authentifizierung.
 */
public class ALKISApiClient {
    
    private String baseUrl; // Die Basis-URL der ALKIS-API.
    private String jwtToken; // Das JSON Web Token, das nach erfolgreichem Login gespeichert wird.

    /**
     * Konstruktor für den ALKISApiClient.
     *
     * @param baseUrl Die Basis-URL der ALKIS-API (z.B. "http://localhost:5260/api/Flurstueck").
     */
    public ALKISApiClient(String baseUrl) {
        this.baseUrl = baseUrl;
    }

    /**
     * Führt den Login bei der ALKIS-API aus und speichert das erhaltene JWT-Token.
     *
     * @param username Der Benutzername für den Login.
     * @param password Das Passwort für den Login.
     * @return {@code true}, wenn der Login erfolgreich war und ein Token empfangen wurde,
     * {@code false} sonst.
     * @throws Exception Wenn ein Fehler während der HTTP-Kommunikation oder beim Parsen
     * der Antwort auftritt.
     */
    public boolean login(String username, String password) throws Exception {
        String url = baseUrl + "/login"; // Die vollständige URL für den Login-Endpunkt
        // Erstellen des JSON-Bodys für den Login-Request mit Benutzername und Passwort.
        String jsonBody = String.format("{\"username\": \"%s\", \"password\": \"%s\"}", username, password);

        try (CloseableHttpClient httpClient = HttpClients.createDefault()) { // Erstellt einen neuen HTTP-Client, der nach Gebrauch automatisch geschlossen wird.
            HttpPost post = new HttpPost(url); // Erstellt einen HTTP POST Request.
            post.setHeader("Content-Type", "application/json"); // Setzt den Content-Type Header auf JSON.
            post.setEntity(new StringEntity(jsonBody, "UTF-8")); // Setzt den Request-Body als StringEntity mit UTF-8 Kodierung.

            HttpResponse response = httpClient.execute(post); // Führt den HTTP POST Request aus und erhält die Antwort.
            String apiResponse = EntityUtils.toString(response.getEntity(), "UTF-8"); // Liest den Inhalt der API-Antwort als String aus.

            // Extrahieren des JWT-Tokens aus der API-Antwort.
            String token = extractToken(apiResponse);

            if (token != null && !token.isEmpty()) { // Prüft, ob ein gültiges Token extrahiert wurde.
                this.jwtToken = token; // Speichert das erhaltene Token für zukünftige authentifizierte Anfragen.
                return true; // Login war erfolgreich.
            } else {
                return false; // Login war nicht erfolgreich (kein Token oder leeres Token).
            }
        }
    }

    /**
     * Ruft eine Liste aller verfügbaren Gemarkungen von der ALKIS-API ab.
     * Diese Methode erfordert eine vorherige erfolgreiche Authentifizierung.
     *
     * @return Eine {@code ArrayList} von Strings, die die Namen der Gemarkungen enthalten.
     * @throws Exception Wenn ein Fehler während der HTTP-Kommunikation oder beim Parsen
     * der JSON-Antwort auftritt (z.B. wenn das Token ungültig ist).
     */
    public ArrayList<String> getMarkings() throws Exception {
        String jsonString = getWithAuth("/getMarkings"); // Führt einen GET-Request mit Authentifizierung zum Endpunkt "/getMarkings" aus.
        JSONArray arr = new JSONArray(jsonString); // Parsen der JSON-Antwort als Array.
        ArrayList<String> result = new ArrayList<>(); // Liste zur Speicherung der Gemarkungsnamen.
        // Iterieren über das JSON-Array und Extrahieren der "gemarkung"-Strings.
        for (int i = 0; i < arr.length(); i++) {
            JSONObject obj = arr.getJSONObject(i); // Holt jedes JSON-Objekt aus dem Array.
            result.add(obj.getString("gemarkung")); // Fügt den Wert des "gemarkung"-Feldes zur Ergebnisliste hinzu.
        }
        return result; // Gibt die Liste der Gemarkungen zurück.
    }

    /**
     * Führt einen allgemeinen GET-Request an einen bestimmten API-Endpunkt aus
     * und fügt das JWT-Token zur Authentifizierung hinzu.
     *
     * @param endpoint Der spezifische Endpunkt der API (z.B. "/getByCounterNominator").
     * @return Die Antwort der API als String.
     * @throws Exception Wenn ein Fehler während der HTTP-Kommunikation auftritt
     * oder das JWT-Token nicht gesetzt ist.
     */
    public String getWithAuth(String endpoint) throws Exception {
        String url = baseUrl + endpoint; // Die vollständige URL für den Endpunkt.

        try (CloseableHttpClient httpClient = HttpClients.createDefault()) { // Erstellt einen neuen HTTP-Client.
            HttpGet request = new HttpGet(url); // Erstellt einen HTTP GET Request.
            // Hinzufügen des Authorization-Headers mit dem JWT-Token.
            request.setHeader("Authorization", "Bearer " + jwtToken);

            HttpResponse response = httpClient.execute(request); // Führt den HTTP GET Request aus.
            return EntityUtils.toString(response.getEntity(), "UTF-8"); // Liest die Antwort der API als String aus und gibt sie zurück.
        }
    }

    /**
    * Führt einen asynchronen Import per HTTP-POST im Hintergrund aus,
    * ohne den Event Dispatch Thread (GUI-Thread) zu blockieren.
    * <p>
    * Diese Methode verwendet einen einfachen {@link Thread}, um
    * den HTTP-Aufruf von der Benutzeroberfläche zu entkoppeln.
    * Es erfolgt keine Benachrichtigung per Dialog – Fehler und Erfolg
    * müssen ggf. extern abgefangen oder geloggt werden.
    * </p>
    *
    * @throws NullPointerException wenn {@code baseUrl} oder {@code jwtToken} {@code null} ist
    */
   public void makeImportAsync() {
       if (baseUrl == null || jwtToken == null) { // Prüft, ob die Basis-URL oder das JWT-Token null sind.
           throw new NullPointerException("baseUrl und jwtToken dürfen nicht null sein."); // Wirft eine Ausnahme, wenn dies der Fall ist.
       }

       new Thread(() -> { // Erstellt einen neuen Thread für den asynchronen Import.
           String url = baseUrl + "/import"; // Die vollständige URL für den Import-Endpunkt.

           try (CloseableHttpClient httpClient = HttpClients.createDefault()) { // Erstellt einen neuen HTTP-Client.
               HttpPost request = new HttpPost(url); // Erstellt einen HTTP POST Request.
               request.setHeader("Authorization", "Bearer " + jwtToken); // Fügt den Authorization-Header mit dem JWT-Token hinzu.
               httpClient.execute(request); // Führt den HTTP POST Request aus.
           } catch (Exception ex) { // Fängt alle Ausnahmen ab, die während des HTTP-Aufrufs auftreten können.
               ex.printStackTrace(); // Gibt den Stack-Trace der Ausnahme aus.
           }
       }).start(); // Startet den neuen Thread.
   }
    
    /**
     * Sucht nach einem Flurstück mittels Zähler, Nenner und Gemarkung.
     *
     * @param counter Der Zähler der Flurstücksnummer.
     * @param nominator Der Nenner der Flurstücksnummer.
     * @param marking Die Gemarkung des Flurstücks.
     * @return Die JSON-Antwort der API, die das Flurstück beschreibt.
     * @throws Exception Wenn ein Fehler während der API-Anfrage auftritt.
     */
    public String getByCounterNominator(String counter, String nominator, String marking) throws Exception {
        // Erstellt den Endpunkt-String mit URL-kodierten Parametern für Zähler, Nenner und Gemarkung.
        String endpoint = String.format(
                "/getByCounterNominator?counter=%s&nominator=%s&marking=%s",
                encode(counter), encode(nominator), encode(marking)
        );
        return getWithAuth(endpoint); // Führt den authentifizierten GET-Request aus und gibt die Antwort zurück.
    }

    /**
     * Sucht nach einem Flurstück mittels geografischer Koordinaten (X, Y) und Gemarkung.
     *
     * @param x Die X-Koordinate (Easting) des gesuchten Punkts.
     * @param y Die Y-Koordinate (Northing) des gesuchten Punkts.
     * @param marking Die Gemarkung, in der gesucht werden soll.
     * @return Die JSON-Antwort der API, die das Flurstück beschreibt.
     * @throws Exception Wenn ein Fehler während der API-Anfrage auftritt.
     */
    public String getByCoordinate(double x, double y, String marking) throws Exception {
        // Erstellt den Endpunkt-String mit URL-kodierten Parametern für X, Y und Gemarkung.
        String endpoint = String.format(
                "/getByCoordinate?x=%s&y=%s&marking=%s",
                x, y, encode(marking)
        );
        return getWithAuth(endpoint); // Führt den authentifizierten GET-Request aus und gibt die Antwort zurück.
    }

    /**
     * Eine Hilfsmethode zum einfachen Parsen des JWT-Tokens aus einer JSON-Antwort.
     * Diese Methode ist für eine spezifische JSON-Struktur {@code {"token":"..."}} ausgelegt.
     *
     * @param json Die JSON-Antwort als String.
     * @return Das extrahierte JWT-Token als String, oder {@code null}, wenn nicht gefunden.
     */
    private String extractToken(String json) {
        // Sucht nach dem Start des Tokens nach dem "token":" Präfix.
        int i = json.indexOf(":\"");
        if (i == -1) return null; // Wenn ":\" nicht gefunden wurde, ist kein Token vorhanden.
        // Sucht nach dem Ende des Tokens nach dem nächsten Anführungszeichen.
        int j = json.indexOf("\"", i + 2);
        if (j == -1) return null; // Wenn das schließende Anführungszeichen nicht gefunden wurde, ist das Format ungültig.
        return json.substring(i + 2, j); // Gibt den Substring zwischen den Anführungszeichen zurück, der das Token ist.
    }

    /**
     * Hilfsmethode zum URL-Encoding eines Strings.
     * Dies ist notwendig, um Sonderzeichen in URL-Parametern korrekt zu übertragen.
     *
     * @param s Der zu kodierende String.
     * @return Der URL-kodierte String.
     * @throws Exception Wenn die Kodierung fehlschlägt (z.B. unsupported encoding).
     */
    private String encode(String s) throws Exception {
        return java.net.URLEncoder.encode(s, "UTF-8"); // Kodiert den String mit UTF-8 und gibt ihn zurück.
    }

    /**
     * Gibt das aktuell gespeicherte JWT-Token zurück.
     *
     * @return Das JWT-Token als String.
     */
    public String getJwtToken() {
        return jwtToken; // Gibt das gespeicherte JWT-Token zurück.
    }
    
    /**
     * Hauptmethode zum Testen der Funktionalität des ALKISApiClient.
     * Führt einen Login durch, ruft Gemarkungen ab und sucht ein Beispiel-Flurstück.
     *
     * @param args Kommandozeilenargumente (nicht verwendet).
     * @throws Exception Wenn ein Fehler während der Ausführung auftritt.
     */
    public static void main(String[] args) throws Exception {
        // Basis-URL der API (ohne / am Ende!). Muss an die tatsächliche API-URL angepasst werden.
        String apiBaseUrl = "http://localhost:5260/api/Flurstueck"; 

        ALKISApiClient client = new ALKISApiClient(apiBaseUrl); // Erstellt eine neue Instanz des API-Clients.

        // Versucht, sich bei der API anzumelden.
        if (client.login("ALKIS", "ALKIS")) { // Verwendet Standard-Benutzername und -Passwort.
            System.out.println("Login erfolgreich!");
            // Ruft alle Gemarkungen ab.
            ArrayList<String> markings = client.getMarkings();
            // Gibt jede Gemarkung aus.
            markings.forEach(marking -> System.out.println("Gemarkung: " + marking));
            
            // Sucht ein Flurstück anhand von Koordinaten und Gemarkung.
            // Die Koordinaten und die Gemarkung müssen an reale Daten angepasst werden, um ein Ergebnis zu erhalten.
            var test = client.getByCoordinate(47.853549, 9.009628, "Stockach");
            System.out.println("Coordinates: " + test);
            
            // Beispiele für weitere Requests (auskommentiert)
            // String result = client.getByCounterNominator("1", "2", "DEINEGEMARKUNG");
            // System.out.println("Flurstueck: " + result);
        } else {
            System.out.println("Login fehlgeschlagen."); // Ausgabe bei fehlgeschlagenem Login.
        }
    }
}
