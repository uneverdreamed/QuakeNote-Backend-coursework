namespace QuakeNote.Earthquakes.Core.Infrastructure;

public static class TimeExtensions
{
    /// <summary>Npgsql принимает для timestamptz только UTC.</summary>
    public static DateTime ToUtc(this DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}