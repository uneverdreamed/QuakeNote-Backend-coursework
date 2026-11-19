namespace QuakeNote.Reports.Core.Dtos;

/// <summary>Расширенная анкета MSK-64. Все поля необязательные.</summary>
public sealed class DetailedReportDto
{
    /// <summary>Когда произошло событие (если отличается от времени отправки отчёта).</summary>
    public DateTime? EventDate { get; init; }
    public string? Duration { get; init; }
    public string? LocationType { get; init; }
    public string? HumanAction { get; init; }
    public string? OscillationType { get; init; }
    public List<string>? Reactions { get; init; }
    public string? WitnessesCount { get; init; }
    public List<string>? AffectedObjects { get; init; }
    public List<string>? BuildingElements { get; init; }
    public List<string>? ObservationsInside { get; init; }
    public List<string>? OutdoorObjects { get; init; }
    public List<string>? Infrastructure { get; init; }
    public List<string>? SurfacePhenomena { get; init; }
    public string? CrackWidth { get; init; }
    public string? CrackLength { get; init; }
    public List<string>? OtherObservations { get; init; }
    public string? ContactPhone { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactName { get; init; }
    public string? AdditionalInfo { get; init; }
}