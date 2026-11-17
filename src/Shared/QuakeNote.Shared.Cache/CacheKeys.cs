namespace QuakeNote.Shared.Cache;

public static class CacheKeys
{
    public const string EarthquakesListPrefix = "earthquakes:list:";

    public static string AuthBlacklist(string jti) => $"auth:blacklist:{jti}";
    public static string EarthquakesList(string filterHash) => $"{EarthquakesListPrefix}{filterHash}";
    public static string EarthquakeDetail(Guid uid) => $"earthquakes:detail:{uid}";
    public static string ReportsStats(Guid earthquakeUid) => $"reports:stats:{earthquakeUid}";
}

public static class CacheTtl
{
    public static readonly TimeSpan EarthquakesList = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan EarthquakeDetail = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan ReportsStats = TimeSpan.FromMinutes(10);
}