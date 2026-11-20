using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using QuakeNote.Earthquakes.Core.Infrastructure;
using QuakeNote.Earthquakes.Core.Services;
using QuakeNote.Earthquakes.Data;
using QuakeNote.Shared.Contracts.Errors;
using QuakeNote.Shared.Grpc.Earthquakes;

namespace QuakeNote.Earthquakes.API.GrpcServices;

/// <summary>gRPC для Verification: поиск события под отчёт и показания станций.</summary>
public class EarthquakesGrpcServicer(EarthquakesDbContext db, StationReadingService readings)
    : EarthquakesGrpc.EarthquakesGrpcBase
{
    private const int MaxWindowMinutes = 24 * 60;
    private const int MaxCandidates = 1000;

    /// <summary>Среди событий в окне ±window_minutes от timestamp выбирает ближайшее к точке (при равенстве — ближайшее по времени).</summary>
    public override async Task<FindNearestEarthquakeReply> FindNearestEarthquake(
        FindNearestEarthquakeRequest request, ServerCallContext context)
    {
        if (request.Timestamp is null)
            throw Invalid("timestamp обязателен.");
        if (!(request.Latitude >= -90 && request.Latitude <= 90))
            throw Invalid("latitude должна быть от -90 до 90.");
        if (!(request.Longitude >= -180 && request.Longitude <= 180))
            throw Invalid("longitude должна быть от -180 до 180.");
        if (request.WindowMinutes < 1 || request.WindowMinutes > MaxWindowMinutes)
            throw Invalid($"window_minutes должно быть от 1 до {MaxWindowMinutes}.");

        var center = request.Timestamp.ToDateTime(); // всегда UTC
        var from = center.AddMinutes(-request.WindowMinutes);
        var to = center.AddMinutes(request.WindowMinutes);

        var candidates = await db.Earthquakes.AsNoTracking()
            .Where(e => e.OccurredAt >= from && e.OccurredAt <= to)
            .Select(e => new { e.Uid, e.OccurredAt, e.Magnitude, e.EpicenterLat, e.EpicenterLon, e.Depth })
            .Take(MaxCandidates)
            .ToListAsync(context.CancellationToken);

        if (candidates.Count == 0)
            return new FindNearestEarthquakeReply { Found = false };

        var lat = (decimal)request.Latitude;
        var lon = (decimal)request.Longitude;
        var best = candidates
            .Select(c => new
            {
                Event = c,
                Distance = GeoMath.HaversineKm(lat, lon, c.EpicenterLat, c.EpicenterLon),
                TimeDiff = Math.Abs((c.OccurredAt.ToUtc() - center).Ticks)
            })
            .OrderBy(x => x.Distance).ThenBy(x => x.TimeDiff)
            .First().Event;

        return new FindNearestEarthquakeReply
        {
            Found = true,
            EarthquakeUid = best.Uid.ToString(),
            OccurredAt = Timestamp.FromDateTime(best.OccurredAt.ToUtc()),
            Magnitude = (double)best.Magnitude,
            EpicenterLat = (double)best.EpicenterLat,
            EpicenterLon = (double)best.EpicenterLon,
            DepthKm = (double)best.Depth
        };
    }

    /// <summary>Показания всех станций для события (ближайшие к эпицентру первыми).</summary>
    public override async Task<GetStationReadingsReply> GetStationReadings(
        GetStationReadingsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.EarthquakeUid, out var uid))
            throw Invalid("earthquake_uid должен быть UUID.");

        try
        {
            var list = await readings.GetByEarthquakeAsync(uid, context.CancellationToken);

            var reply = new GetStationReadingsReply();
            reply.Readings.AddRange(list.Select(r => new StationReading
            {
                StationUid = r.StationUid.ToString(),
                StationLat = (double)r.StationLat,
                StationLon = (double)r.StationLon,
                Pga = (double)r.Pga,
                Pgv = (double)r.Pgv,
                DistanceToEpicenterKm = (double)r.DistanceToEpicenterKm
            }));
            return reply;
        }
        catch (NotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    private static RpcException Invalid(string message) => new(new Status(StatusCode.InvalidArgument, message));
}