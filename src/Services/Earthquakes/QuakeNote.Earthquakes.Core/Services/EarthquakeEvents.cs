using QuakeNote.Earthquakes.Data.Entities;
using QuakeNote.Shared.Contracts.Events;

namespace QuakeNote.Earthquakes.Core.Services;

internal static class EarthquakeEvents
{
    public static EarthquakeCreatedEvent ToCreatedEvent(this Earthquake e)
        => new(e.Uid, e.OccurredAt, e.Magnitude, e.EpicenterLat, e.EpicenterLon);
}