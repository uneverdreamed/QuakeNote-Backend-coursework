namespace QuakeNote.Earthquakes.Core.Abstractions;

/// <summary>Статистика отчётов по событию (ответ Reports).</summary>
public sealed record ReportsStats(
    int TotalCount, int VerifiedCount, int OutlierCount, int UnverifiedCount, int PendingCount, double? MeanIntensity);

public interface IReportsGrpcClient
{
    Task<ReportsStats> GetStatsAsync(Guid earthquakeUid, CancellationToken ct = default);
}