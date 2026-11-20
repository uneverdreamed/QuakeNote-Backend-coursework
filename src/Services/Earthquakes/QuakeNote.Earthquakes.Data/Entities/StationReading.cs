namespace QuakeNote.Earthquakes.Data.Entities;

/// <summary>Показания станции для конкретного события (связь «многие ко многим»).</summary>
public class StationReading
{
    public int Id { get; set; }

    public int EarthquakeId { get; set; }
    public Earthquake Earthquake { get; set; } = null!;

    public int StationId { get; set; }
    public SeismicStation Station { get; set; } = null!;

    /// <summary>Пиковое ускорение, см/с².</summary>
    public decimal Pga { get; set; }

    /// <summary>Пиковая скорость, см/с.</summary>
    public decimal Pgv { get; set; }

    /// <summary>Пиковое смещение, см.</summary>
    public decimal? Pgd { get; set; }

    public decimal? AriasIntensity { get; set; }
    public DateTime PWaveArrivalAt { get; set; }
    public decimal DistanceToEpicenterKm { get; set; }
}