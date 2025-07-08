using ALKISService.Repository;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Globalization;
using System.Xml.Linq;
using System.Xml;

namespace ALKISService.Services
{
    public interface IGeometryService
    {
        List<FlurstueckEntity> ParseFlurstueckeFast(string xmlPath);
    }

    public class GeometryService : IGeometryService
    {

        public List<FlurstueckEntity> ParseFlurstueckeFast(string xmlPath)
        {
            List<FlurstueckEntity> result = new List<FlurstueckEntity>();
            using var reader = XmlReader.Create(xmlPath, new XmlReaderSettings { IgnoreWhitespace = true });
            var dateiname = Path.GetFileName(xmlPath);

            while (reader.Read())
            {
                // Finde AX_Flurstueck (Start)
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "AX_Flurstueck")
                {
                    string zaehler = null, nenner = "0";
                    double amtlicheFlaeche = 0;
                    List<Coordinate> coords = new();

                    using (var flurstueckSubtree = reader.ReadSubtree())
                    {
                        flurstueckSubtree.Read(); 
                        while (flurstueckSubtree.Read())
                        {
                            if (flurstueckSubtree.NodeType == XmlNodeType.Element)
                            {
                                // Speziell für flurstuecksnummer
                                if (flurstueckSubtree.NodeType == XmlNodeType.Element && flurstueckSubtree.LocalName == "AX_Flurstuecksnummer")
                                {
                                    using var nummerSubtree = flurstueckSubtree.ReadSubtree();
                                    nummerSubtree.Read();
                                    string name = "";
                                    while (nummerSubtree.Read())
                                    {
                                        if(nummerSubtree.NodeType == XmlNodeType.Element)
                                        {
                                            name = nummerSubtree.LocalName.ToLowerInvariant();
                                        }
                                        if (nummerSubtree.NodeType == XmlNodeType.Text)
                                        {
                                            string value = nummerSubtree.Value.Trim();
                                            if (name == "zaehler")
                                                zaehler = value;
                                            else if (name == "nenner")
                                                nenner = value;
                                        }
                                    }
                                }

                                else if (flurstueckSubtree.LocalName == "amtlicheFlaeche")
                                {
                                    var str = flurstueckSubtree.ReadElementContentAsString().Trim().Replace(",", ".");
                                    double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out amtlicheFlaeche);
                                }
                                else if (flurstueckSubtree.LocalName == "posList")
                                {
                                    var posListStr = flurstueckSubtree.ReadElementContentAsString().Trim();
                                    var values = posListStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    for (int i = 0; i < values.Length - 1; i += 2)
                                    {
                                        if (double.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                                            double.TryParse(values[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
                                            coords.Add(new Coordinate(x, y));
                                    }
                                }
                            }
                        }
                    }

                    // Validierung & Yield
                    if (!string.IsNullOrEmpty(zaehler) && coords.Count >= 3)
                    {
                        if (!coords[0].Equals(coords[^1]))
                            coords.Add(coords[0]);
                        var polygon = new Polygon(new LinearRing(coords.ToArray()));
                        var wktWriter = new WKTWriter();
                        result.Add(new FlurstueckEntity
                        {
                            dateiname = dateiname,
                            zaehler = zaehler,
                            nenner = nenner,
                            wkt = wktWriter.Write(polygon),
                            amtlicheFlaeche = amtlicheFlaeche
                        });
                    }
                }
            }
            return result;
        }

    }

}
