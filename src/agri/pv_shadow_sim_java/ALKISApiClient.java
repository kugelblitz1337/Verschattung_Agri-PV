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
    
    private String baseUrl; // Die Basis-URL der ALKIS-API
    private String jwtToken; // Das JSON Web Token, das nach erfolgreichem Login gespeichert wird

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
        // Erstellen des JSON-Bodys für den Login-Request
        String jsonBody = String.format("{\"username\": \"%s\", \"password\": \"%s\"}", username, password);

        try (CloseableHttpClient httpClient = HttpClients.createDefault()) {
            HttpPost post = new HttpPost(url);
            post.setHeader("Content-Type", "application/json"); // Setzen des Content-Type Headers auf JSON
            post.setEntity(new StringEntity(jsonBody, "UTF-8")); // Setzen des Request-Bodys

            HttpResponse response = httpClient.execute(post); // Ausführen des HTTP POST Requests
            String apiResponse = EntityUtils.toString(response.getEntity(), "UTF-8"); // Auslesen der API-Antwort

            // Extrahieren des JWT-Tokens aus der API-Antwort
            String token = extractToken(apiResponse);

            if (token != null && !token.isEmpty()) {
                this.jwtToken = token; // Speichern des Tokens
                return true;
            } else {
                return false;
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
        String jsonString = getWithAuth("/getMarkings"); // Führt einen GET-Request mit Authentifizierung aus
        JSONArray arr = new JSONArray(jsonString); // Parsen der JSON-Antwort als Array
        ArrayList<String> result = new ArrayList<>();
        // Iterieren über das JSON-Array und Extrahieren der "gemarkung"-Strings
        for (int i = 0; i < arr.length(); i++) {
            JSONObject obj = arr.getJSONObject(i);
            result.add(obj.getString("gemarkung"));
        }
        return result;
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
        String url = baseUrl + endpoint; // Die vollständige URL für den Endpunkt

        try (CloseableHttpClient httpClient = HttpClients.createDefault()) {
            HttpGet request = new HttpGet(url);
            // Hinzufügen des Authorization-Headers mit dem JWT-Token
            request.setHeader("Authorization", "Bearer " + jwtToken);

            HttpResponse response = httpClient.execute(request); // Ausführen des HTTP GET Requests
            return EntityUtils.toString(response.getEntity(), "UTF-8"); // Auslesen und Zurückgeben der Antwort
        }
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
        // Erstellen des Endpunkt-Strings mit URL-kodierten Parametern
        String endpoint = String.format(
                "/getByCounterNominator?counter=%s&nominator=%s&marking=%s",
                encode(counter), encode(nominator), encode(marking)
        );
        return getWithAuth(endpoint); // Führt den authentifizierten GET-Request aus
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
        // Erstellen des Endpunkt-Strings mit URL-kodierten Parametern
        String endpoint = String.format(
                "/getByCoordinate?x=%s&y=%s&marking=%s",
                x, y, encode(marking)
        );
        return getWithAuth(endpoint); // Führt den authentifizierten GET-Request aus
    }

    /**
     * Eine Hilfsmethode zum einfachen Parsen des JWT-Tokens aus einer JSON-Antwort.
     * Diese Methode ist für eine spezifische JSON-Struktur {@code {"token":"..."}} ausgelegt.
     *
     * @param json Die JSON-Antwort als String.
     * @return Das extrahierte JWT-Token als String, oder {@code null}, wenn nicht gefunden.
     */
    private String extractToken(String json) {
        // Sucht nach dem Start des Tokens nach dem "token":" Präfix
        int i = json.indexOf(":\"");
        if (i == -1) return null; // Wenn ":\" nicht gefunden wurde
        // Sucht nach dem Ende des Tokens nach dem nächsten Anführungszeichen
        int j = json.indexOf("\"", i + 2);
        if (j == -1) return null; // Wenn das schließende Anführungszeichen nicht gefunden wurde
        return json.substring(i + 2, j); // Gibt den Substring zwischen den Anführungszeichen zurück
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
        return java.net.URLEncoder.encode(s, "UTF-8");
    }

    /**
     * Gibt das aktuell gespeicherte JWT-Token zurück.
     *
     * @return Das JWT-Token als String.
     */
    public String getJwtToken() {
        return jwtToken;
    }
    
    /**
     * Hauptmethode zum Testen der Funktionalität des ALKISApiClient.
     * Führt einen Login durch, ruft Gemarkungen ab und sucht ein Beispiel-Flurstück.
     *
     * @param args Kommandozeilenargumente (nicht verwendet).
     * @throws Exception Wenn ein Fehler während der Ausführung auftritt.
     */
    public static void main(String[] args) throws Exception {
        // Basis-URL der API (ohne / am Ende!)
        String apiBaseUrl = "http://localhost:5260/api/Flurstueck"; // Anpassen!

        ALKISApiClient client = new ALKISApiClient(apiBaseUrl);

        // Versucht, sich bei der API anzumelden
        if (client.login("ALKIS", "ALKIS")) { // Platzhalter-Benutzername und -Passwort
            System.out.println("Login erfolgreich!");
            // Ruft alle Gemarkungen ab
            ArrayList<String> markings = client.getMarkings();
            // Gibt jede Gemarkung aus
            markings.forEach(marking -> System.out.println("Gemarkung: " + marking));
            
            // Sucht ein Flurstück anhand von Koordinaten und Gemarkung
            var test = client.getByCoordinate(47.853549, 9.009628, "Stockach");
            System.out.println("Coordinates: " + test);
            
            // Beispiele für weitere Requests (auskommentiert)
            // String result = client.getByCounterNominator("1", "2", "DEINEGEMARKUNG");
            // System.out.println("Flurstueck: " + result);
        } else {
            System.out.println("Login fehlgeschlagen.");
        }
    }
}
