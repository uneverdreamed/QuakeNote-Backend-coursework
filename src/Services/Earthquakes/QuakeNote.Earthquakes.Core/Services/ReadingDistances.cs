using QuakeNote.Earthquakes.Core.Infrastructure;
using QuakeNote.Earthquakes.Data.Entities;

namespace QuakeNote.Earthquakes.Core.Services;

internal static class ReadingDistances
{
    /// <summary>Пересчитывает расстояние от эпицентра до станций. Нужны загруженные Readings с Station.</summary>
    public static void Recalculate(Earthquake earthquake)
    {
        foreach (var reading in earthquake.Readings)
        {
            reading.DistanceToEpicenterKm = GeoMath.HaversineKm(
                earthquake.EpicenterLat, earthquake.EpicenterLon,
                reading.Station.Latitude, reading.Station.Longitude);
        }
    }
}