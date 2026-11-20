namespace QuakeNote.Earthquakes.Data.Entities;

/// <summary>Агрегированная оценка события (одна запись на событие).</summary>
public class EventAssessment
{
    public int Id { get; set; }

    public int EarthquakeId { get; set; }
    public Earthquake Earthquake { get; set; } = null!;

    /// <summary>Интенсивность по приборным данным (считает Verification по GMICE).</summary>
    public decimal? InstrumentalIntensity { get; set; }

    /// <summary>Интенсивность по отчётам пользователей (берём у Reports).</summary>
    public decimal? MacroseismicIntensity { get; set; }

    /// <summary>confirmed | disputed | insufficient_data</summary>
    public string VerificationStatus { get; set; } = "insufficient_data";

    public int ReportsCount { get; set; }
    public DateTime LastRecomputedAt { get; set; }
}