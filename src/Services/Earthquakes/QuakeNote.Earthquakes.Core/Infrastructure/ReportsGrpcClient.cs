using QuakeNote.Earthquakes.Core.Abstractions;
using QuakeNote.Shared.Grpc.Reports;

namespace QuakeNote.Earthquakes.Core.Infrastructure;

/// <summary>Вызывает gRPC-сервис Reports. Сам gRPC-клиент и заголовок секрета настраиваются в API.</summary>
public sealed class ReportsGrpcClient(ReportsGrpc.ReportsGrpcClient client) : IReportsGrpcClient
{
    public async Task<ReportsStats> GetStatsAsync(Guid earthquakeUid, CancellationToken ct = default)
    {
        var reply = await client.GetReportsStatsByEarthquakeAsync(
            new ReportsStatsRequest { EarthquakeUid = earthquakeUid.ToString() },
            cancellationToken: ct);

        return new ReportsStats(
            reply.TotalCount,
            reply.VerifiedCount,
            reply.OutlierCount,
            reply.UnverifiedCount,
            reply.PendingCount,
            reply.HasMeanIntensity ? reply.MeanIntensity : (double?)null);
    }
}