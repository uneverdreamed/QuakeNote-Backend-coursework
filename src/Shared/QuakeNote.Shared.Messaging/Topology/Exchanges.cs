namespace QuakeNote.Shared.Messaging.Topology;

public static class Exchanges
{
    public const string Reports = "reports.events";
    public const string Earthquakes = "earthquakes.events";
    public const string Auth = "auth.events";
}

public static class RoutingKeys
{
    public const string ReportSubmitted = "report.submitted";
    public const string ReportUpdated = "report.updated";
    public const string ReportMatched = "report.matched";
    public const string EarthquakeCreated = "earthquake.created";
    public const string AuthCodeRequested = "auth.code_requested";
}