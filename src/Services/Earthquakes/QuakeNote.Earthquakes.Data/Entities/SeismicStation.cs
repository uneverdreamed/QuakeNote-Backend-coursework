namespace QuakeNote.Earthquakes.Data.Entities;

/// <summary>Сейсмическая станция (справочник).</summary>
public class SeismicStation
{
    public int Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();

    /// <summary>Технический код станции, уникальный.</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Region { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;

    public List<StationReading> Readings { get; set; } = [];
}