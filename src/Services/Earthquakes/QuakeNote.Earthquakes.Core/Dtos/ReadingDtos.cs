namespace QuakeNote.Earthquakes.Core.Dtos;

/// <summary>Показания станции для события.</summary>
public sealed record StationReadingDto(
    Guid StationUid,
    string StationCode,
    string StationName,
    decimal StationLat,
    decimal StationLon,
    decimal Pga,
    decimal Pgv,
    decimal? Pgd,
    decimal? AriasIntensity,
    DateTime PWaveArrivalAt,
    decimal DistanceToEpicenterKm);

/// <summary>Показания одной станции при загрузке.</summary>
/// <param name="Pga">Пиковое ускорение, см/с².</param>
/// <param name="Pgv">Пиковая скорость, см/с.</param>
/// <param name="DistanceToEpicenterKm">Если не задано, считается по координатам станции и эпицентра.</param>
public sealed record ReadingInputDto(
    Guid? StationUid,
    decimal? Pga,
    decimal? Pgv,
    decimal? Pgd,
    decimal? AriasIntensity,
    DateTime? PWaveArrivalAt,
    decimal? DistanceToEpicenterKm = null);

/// <summary>Загрузка показаний: существующие записи станций обновляются, новые добавляются.</summary>
public sealed record UpsertReadingsDto(List<ReadingInputDto>? Readings);

/// <summary>Результат инструментального расчёта от Verification.</summary>
/// <param name="InstrumentalIntensity">Интенсивность по приборным данным (null — данных нет).</param>
public sealed record RecomputeAssessmentRequest(decimal? InstrumentalIntensity);