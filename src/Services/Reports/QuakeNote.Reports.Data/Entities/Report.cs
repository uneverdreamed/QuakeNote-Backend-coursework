namespace QuakeNote.Reports.Data.Entities;

/// <summary>Базовый отчёт «я ощутил землетрясение».</summary>
public class Report
{
    public int Id { get; set; }

    /// <summary>Внешний идентификатор (наружу и в межсервисных ссылках отдаётся только он).</summary>
    public Guid Uid { get; set; } = Guid.NewGuid();

    /// <summary>users.uid из Auth; null для анонимного отчёта. Внешнего ключа нет (другая БД).</summary>
    public Guid? UserUid { get; set; }

    /// <summary>earthquakes.uid; заполняется после успешного сопоставления.</summary>
    public Guid? EarthquakeUid { get; set; }

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    /// <summary>Самозаявленная интенсивность по MSK-64, 1..10.</summary>
    public short FeltIntensity { get; set; }

    public string? RegionName { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Время последнего изменения; null, если отчёт не редактировали.</summary>
    public DateTime? UpdatedAt { get; set; }

    public DateTime? MatchedAt { get; set; }

    /// <summary>pending | verified | outlier | unverified</summary>
    public string ValidationStatus { get; set; } = "pending";

    public decimal? ExpectedIntensity { get; set; }
    public decimal? Deviation { get; set; }

    public DetailedReport? Detail { get; set; }
}