using QuakeNote.Reports.Data.Entities;
using QuakeNote.Shared.Contracts.Events;

namespace QuakeNote.Reports.Core.Services;

internal static class ReportEvents
{
    public static ReportSubmittedEvent ToSubmittedEvent(this Report r)
        => new(r.Uid, r.UserUid, r.Latitude, r.Longitude, r.FeltIntensity, r.CreatedAt);

    public static ReportUpdatedEvent ToUpdatedEvent(this Report r, Guid? previousEarthquakeUid)
        => new(r.Uid, previousEarthquakeUid, r.Latitude, r.Longitude, r.FeltIntensity, r.CreatedAt);
}