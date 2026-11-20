namespace QuakeNote.Earthquakes.Core.Infrastructure;

public static class GeoMath
{
    private const double EarthRadiusKm = 6371.0088;

    /// <summary>Расстояние по дуге большого круга между двумя точками, км (формула гаверсинусов).</summary>
    public static decimal HaversineKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        var phi1 = ToRadians((double)lat1);
        var phi2 = ToRadians((double)lat2);
        var dPhi = phi2 - phi1;
        var dLambda = ToRadians((double)(lon2 - lon1));

        var a = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2)
              + Math.Cos(phi1) * Math.Cos(phi2) * Math.Sin(dLambda / 2) * Math.Sin(dLambda / 2);
        var km = 2 * EarthRadiusKm * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        return Math.Round((decimal)km, 2);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}