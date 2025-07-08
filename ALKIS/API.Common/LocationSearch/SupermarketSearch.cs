using API.Common.Database;
using API.Common.DTO;
using API.Common.Entities;
using API.Common.Interfaces;
using API.Common.LocationSearch;
using Newtonsoft.Json;
using OpenQA.Selenium.Interactions.Internal;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API.Common
{
    internal class SupermarketSearch : ISupermarketSearch
    {
        private readonly ILogger _logger;
        private readonly IEFConnector _connector;
        private readonly Dictionary<string, Tuple<double,double>> _cache = new Dictionary<string, Tuple<double, double>>();


        public SupermarketSearch(ILogger logger, IEFConnector connector)
        {
            _logger = logger;
            _connector = connector;
        }

        /// <summary>
        /// Alle Supermärkte im Umkreis finden
        /// </summary>
        /// <param name="qryList">Suchbegriffe</param>
        /// <param name="postCode">Postleitzahl</param>
        /// <param name="radiuskm">Radius in Kilometer</param>
        /// <returns></returns>
        public List<Supermarket> RadiusSearchAddressQry(List<string> qry, string postCode, double radiuskm)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            // Koordinaten der Adresse abrufen
            var coordinates = GetCoordinates(postCode);
            stopwatch.Stop();
            _logger.LogHint("RadiusSearchAddressQry" + stopwatch.ElapsedMilliseconds.ToString(), Enum.APITypes.Common);

            if (coordinates != null && !(coordinates.Item1 == 0.0 && coordinates.Item2 == 0.0))
            {
                // Koordinaten des Umkreises berechnen
                var circleCoordinates = CalculateCircleCoordinates(coordinates.Item1, coordinates.Item2, radiuskm);

                return GetSupermarketsInRadius(circleCoordinates.Item1, circleCoordinates.Item2, circleCoordinates.Item3, circleCoordinates.Item4, coordinates.Item1, coordinates.Item2, qry).Where(s => s.Distance <= radiuskm).ToList();
            }
            else
            {
                _logger.LogHint("Fehler beim Abrufen der Koordinaten.", Enum.APITypes.Common);
                return new List<Supermarket>();
            }
        }

        /// <summary>
        /// Für alle Suchbegriffe die Supermärkte finden
        /// </summary>
        /// <param name="lat1">Latidude 1</param>
        /// <param name="lat2">Latidude 2</param>
        /// <param name="long1">Longitude 1</param>
        /// <param name="long2">Longitude 2</param>
        /// <param name="originalLat">Original Latidude</param>
        /// <param name="originalLong">Original Longitude</param>
        /// <param name="qryList">Suchbegriff Liste</param>
        /// <returns>Liste von Supermärkten</returns>
        private List<Supermarket> GetSupermarketsInRadius(double lat1, double lat2, double long1, double long2, double originalLat, double originalLong, List<string> qryList)
        {
            List<Supermarket> supermarkets = new List<Supermarket>();
            foreach (var qry in qryList)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                supermarkets.AddRange(GetSupermarketsInRadius(lat1, lat2, long1, long2, originalLat, originalLong, qry));
                stopwatch.Stop();
                _logger.LogHint("GetSupermarketsInRadius" + stopwatch.ElapsedMilliseconds.ToString(), Enum.APITypes.Common);
            }
            return supermarkets.OrderBy(s => s.Distance).Take(5).ToList();
        }

        /// <summary>
        /// Für alle Suchbegriffe die Supermärkte finden
        /// </summary>
        /// <param name="lat1">Latidude 1</param>
        /// <param name="lat2">Latidude 2</param>
        /// <param name="long1">Longitude 1</param>
        /// <param name="long2">Longitude 2</param>
        /// <param name="originalLat">Original Latidude</param>
        /// <param name="originalLong">Original Longitude</param>
        /// <param name="qry">Suchbegriff</param>
        /// <returns>Liste von Supermärkten</returns>
        private List<Supermarket> GetSupermarketsInRadius(double lat1, double lat2, double long1, double long2, double originalLat, double originalLong, string qry)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var htmlContent = CommonWebClient.WebClient.GetHTMLCode($"https://nominatim.openstreetmap.org/search?q={qry}&viewbox={long1.ToString().Replace(",", ".")},{lat1.ToString().Replace(",", ".")},{long2.ToString().Replace(",", ".")},{lat2.ToString().Replace(",", ".")}&bounded=1&format=json&limit=50");

            stopwatch.Stop();
            _logger.LogHint("Abruf Daten " + stopwatch.ElapsedMilliseconds, Enum.APITypes.Common);
            stopwatch.Restart();
            var locations = JsonConvert.DeserializeObject<List<LocationSearch.Location>>(htmlContent);
            stopwatch.Stop();
            _logger.LogHint("Umwandeln Json " + stopwatch.ElapsedMilliseconds, Enum.APITypes.Common);
            List<Supermarket> supermarkets = new List<Supermarket>();
            if (locations != null)
            {
                stopwatch.Restart();
                foreach (var location in locations)
                {
                    var distance = CalculateDistance(originalLat, originalLong, location.lat, location.lon);
                    supermarkets.Add(GetCommonSupermarket(location, Math.Round(distance, 2)));
                }
                stopwatch.Stop();
                _logger.LogHint("location " + stopwatch.ElapsedMilliseconds, Enum.APITypes.Common);
            }
            return supermarkets;
        }
        /// <summary>
        /// Distanz berechnen
        /// </summary>
        /// <param name="lat1">Latidude 1</param>
        /// <param name="lat2">Latidude 2</param>
        /// <param name="long1">Longitude 1</param>
        /// <param name="long2">Longitude 2</param>
        /// <returns>Entfernung in km</returns>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double earthRadius = 6371; // Radius der Erde in Kilometern

            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c; // Entfernung in Kilometern
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
        /// <summary>
        /// Common Supermärkte erstellen
        /// </summary>
        /// <param name="location">Location von OpenStreetMap</param>
        /// <param name="distance">Distanz</param>
        /// <returns>Supermarkt Objekt</returns>
        private Supermarket GetCommonSupermarket(LocationSearch.Location location, double distance)
        {
            var supermarket = new Supermarket()
            {
                Title = location.display_name,
                lat = location.lat,
                lon = location.lon,
                Distance = distance
            };
            return supermarket;
        }

        /// <summary>
        /// Koordinaten zur Postleitzahl ermitteln - Suche im Cache, Suche in der Datenbank, Suche in OpenStreetMap
        /// </summary>
        /// <param name="plz">Postleitzahl</param>
        /// <returns>Latidude and Longitude</returns>
        private Tuple<double, double> GetCoordinates(string plz)
        {
            //Versuchen Daten im Cache zu finden
            var locationCache = _cache.Where(p => p.Key == plz).FirstOrDefault();
            if (locationCache.Value != null && !(locationCache.Value.Item1 == 0.0 && locationCache.Value.Item2 == 0.0))
            {
                return locationCache.Value;
            }
                
            //Versuchen Koordinaten für Postleitzahl aus Datenbank zu holen, da schneller
            var locationDb = GetCoordinatesFromDB(plz);
            if (locationDb != null && !(locationDb.Item1 == 0.0 && locationDb.Item2 == 0.0))
            {
                _cache.Add(plz, locationDb);
                return locationDb;
            }
            //Versuchen Koordinaten für Postleitzahl aus OpenStreetMap zu holen
            var locationOpenStreet = GetCoordinatesFromOpenStreet(plz);
            if (locationOpenStreet != null && !(locationOpenStreet.Item1 == 0.0 && locationOpenStreet.Item2 == 0.0))
            {
                _cache.Add(plz, locationOpenStreet);
                return locationOpenStreet;
            }
            return Tuple.Create(0.0, 0.0);
        }

        /// <summary>
        /// Koordianten anhand von Postleitzahl ermitteln
        /// </summary>
        /// <param name="postCode">Postleitzahl</param>
        /// <returns>Longitude, Latitude</returns>
        private Tuple<double, double> GetCoordinatesFromDB(string postCode)
        {
            //Zuerst versuchen in Datenbank zu finden
            var dbLocationsRepo = _connector.GetRepository<Entities.Location>();

            var dbLocations = dbLocationsRepo.GetAll();
            var dbLocation = dbLocations.Where(l => l.plz_code == postCode).FirstOrDefault();
            if (dbLocation != null)
            {
                return Tuple.Create(Convert.ToDouble(dbLocation.lat), Convert.ToDouble(dbLocation.lon));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Koordianten anhand von Postleitzahl ermitteln - OpenStreetMap
        /// </summary>
        /// <param name="postCode">Postleitzahl</param>
        /// <returns>Longitude, Latitude</returns>
        private Tuple<double, double> GetCoordinatesFromOpenStreet(string postCode)
        {
            // Http Client verwenden
            var htmlContent = CommonWebClient.WebClient.GetHTMLCode($"https://nominatim.openstreetmap.org/search?postalcode={postCode}&country=DE&format=json");

            var location = JsonConvert.DeserializeObject<List<LocationSearch.Location>>(htmlContent)?.FirstOrDefault();
            if (location != null)
            {
                return Tuple.Create(Convert.ToDouble(location.lat), Convert.ToDouble(location.lon));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Kreiskoordinaten berechnen für Umkreissuche
        /// </summary>
        /// <param name="latitude">Latitude von Postleitzahl</param>
        /// <param name="longitude">Longitude von Postleitzahl</param>
        /// <param name="radiusInKm">Radius in km</param>
        /// <returns></returns>
        private Tuple<double, double, double, double> CalculateCircleCoordinates(double latitude, double longitude, double radiusInKm)
        {
            double lat1 = latitude - (radiusInKm / 111);
            double lat2 = latitude + (radiusInKm / 111);
            double lon1 = longitude - (radiusInKm / (111 * Math.Cos(latitude * (Math.PI / 180))));
            double lon2 = longitude + (radiusInKm / (111 * Math.Cos(latitude * (Math.PI / 180))));

            return Tuple.Create(lat1, lat2, lon1, lon2);
        }
    }
}
