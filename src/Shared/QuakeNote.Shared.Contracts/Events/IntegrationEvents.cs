namespace QuakeNote.Shared.Contracts.Events;

public record ReportSubmittedEvent(
    Guid ReportUid,
    Guid? UserUid,
    decimal Latitude,
    decimal Longitude,
    short FeltIntensity,
    DateTimeOffset CreatedAt);

public record ReportUpdatedEvent(
    Guid ReportUid,
    Guid? PreviousEarthquakeUid,
    decimal Latitude,
    decimal Longitude,
    short FeltIntensity,
    DateTimeOffset CreatedAt);

public record ReportMatchedEvent(
    Guid ReportUid,
    Guid? UserUid,
    Guid? EarthquakeUid,
    string Decision);

public record EarthquakeCreatedEvent(
    Guid EarthquakeUid,
    DateTimeOffset OccurredAt,
    decimal Magnitude,
    decimal EpicenterLat,
    decimal EpicenterLon);

public record AuthCodeRequestedEvent(
    string Email,
    string Code,
    DateTimeOffset ExpiresAt);