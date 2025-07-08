using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

public class CoordinateTransformService
{
    private readonly ICoordinateTransformation _wgs84ToUtmTransform;


    public CoordinateTransformService()
    {
        // Definiere WGS84 (EPSG: 4326) als Quell-Koordinatensystem
        var wgs84 = GeographicCoordinateSystem.WGS84;

        // Definiere ETRS89 UTM Zone 32T (EPSG: 25832) als Ziel-Koordinatensystem
        var etrs89Utm32T = ProjectedCoordinateSystem.WGS84_UTM(32, true);  // 'false' für ETRS89

        // Erstelle eine Transformation von WGS84 nach ETRS89 UTM32T
        var transformationFactory = new CoordinateTransformationFactory();
        _wgs84ToUtmTransform = transformationFactory.CreateFromCoordinateSystems(wgs84, etrs89Utm32T);
    }

    public double[] TransformToUtm(double latitude, double longitude)
    {
        // Transformiere WGS84 (Latitude, Longitude) nach ETRS89 UTM32T
        return _wgs84ToUtmTransform.MathTransform.Transform(new[] { longitude, latitude });
    }

}
