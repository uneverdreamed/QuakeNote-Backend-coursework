namespace QuakeNote.Reports.Data.Entities;

/// <summary>Расширенная анкета MSK-64 (0..1 на отчёт). Все поля необязательные.</summary>
public class DetailedReport
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public Report Report { get; set; } = null!;

    public DateTime? EventDate { get; set; }
    public string? Duration { get; set; }
    public string? LocationType { get; set; }
    public string? HumanAction { get; set; }
    public string? OscillationType { get; set; }
    public List<string> Reactions { get; set; } = [];
    public string? WitnessesCount { get; set; }
    public List<string> AffectedObjects { get; set; } = [];
    public List<string> BuildingElements { get; set; } = [];
    public List<string> ObservationsInside { get; set; } = [];
    public List<string> OutdoorObjects { get; set; } = [];
    public List<string> Infrastructure { get; set; } = [];
    public List<string> SurfacePhenomena { get; set; } = [];
    public string? CrackWidth { get; set; }
    public string? CrackLength { get; set; }
    public List<string> OtherObservations { get; set; } = [];
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactName { get; set; }
    public string? AdditionalInfo { get; set; }
}