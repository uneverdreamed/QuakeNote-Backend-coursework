using Grpc.Core;
using QuakeNote.Reports.Core.Services;
using QuakeNote.Shared.Grpc.Reports;

namespace QuakeNote.Reports.API.GrpcServices;

/// <summary>Внутренний gRPC-сервис: агрегат отчётов по событию (для Earthquakes).</summary>
public class ReportsGrpcServicer(ReportStatsService stats) : ReportsGrpc.ReportsGrpcBase
{
    public override async Task<ReportsStatsReply> GetReportsStatsByEarthquake(
        ReportsStatsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.EarthquakeUid, out var earthquakeUid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "earthquake_uid должен быть корректным UUID."));

        var s = await stats.GetAsync(earthquakeUid, context.CancellationToken);

        return new ReportsStatsReply
        {
            TotalCount = s.TotalCount,
            VerifiedCount = s.VerifiedCount,
            OutlierCount = s.OutlierCount,
            UnverifiedCount = s.UnverifiedCount,
            PendingCount = s.PendingCount,
            HasMeanIntensity = s.MeanIntensity.HasValue,
            MeanIntensity = s.MeanIntensity ?? 0
        };
    }
}