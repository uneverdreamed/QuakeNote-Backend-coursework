namespace QuakeNote.Earthquakes.Data.Entities;

/// <summary>Сейсмическое событие.</summary>
public class Earthquake
{
    public int Id { get; set; }

    /// <summary>Внешний идентификатор (его отдаём наружу и используем в других сервисах).</summary>
    public Guid Uid { get; set; } = Guid.NewGuid();

    /// <summary>Идентификатор события у источника данных (каталог ФИЦ ЕГС РАН, USGS и т.п.).</summary>
    public string ExternalId { get; set; } = string.Empty;

    public decimal EpicenterLat { get; set; }
    public decimal EpicenterLon { get; set; }

    /// <summary>Глубина очага, км.</summary>
    public decimal Depth { get; set; }

    public decimal Magnitude { get; set; }
    public DateTime OccurredAt { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public EventAssessment? Assessment { get; set; }
    public List<StationReading> Readings { get; set; } = [];
}