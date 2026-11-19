using Microsoft.EntityFrameworkCore;
using QuakeNote.Reports.Core.Dtos;
using QuakeNote.Reports.Data;
using QuakeNote.Shared.Cache;
using QuakeNote.Shared.Contracts;

namespace QuakeNote.Reports.Core.Services;

public class ReportStatsService(ReportsDbContext db, ICacheService cache)
{
    /// <summary>Cache-aside: сначала Redis, при промахе считаем по БД и кладём в кэш.</summary>
    public Task<ReportsStatsDto> GetAsync(Guid earthquakeUid, CancellationToken ct = default)
        => cache.GetOrCreateAsync(
            CacheKeys.ReportsStats(earthquakeUid),
            CacheTtl.ReportsStats,
            () => ComputeAsync(earthquakeUid, ct));

    /// <summary>Сбрасывает кэш статистики для указанных событий (null пропускаются).</summary>
    public async Task InvalidateAsync(params Guid?[] earthquakeUids)
    {
        foreach (var uid in earthquakeUids.OfType<Guid>().Distinct())
            await cache.RemoveAsync(CacheKeys.ReportsStats(uid));
    }

    private async Task<ReportsStatsDto> ComputeAsync(Guid earthquakeUid, CancellationToken ct)
    {
        var rows = await db.Reports.AsNoTracking()
            .Where(r => r.EarthquakeUid == earthquakeUid)
            .GroupBy(r => r.ValidationStatus)
            .Select(g => new { Status = g.Key, Count = g.Count(), Sum = g.Sum(r => (int)r.FeltIntensity) })
            .ToListAsync(ct);

        int Count(string status) => rows.Where(x => x.Status == status).Sum(x => x.Count);
        int Sum(string status) => rows.Where(x => x.Status == status).Sum(x => x.Sum);

        // Средняя считается по отчётам без выбросов: verified + unverified
        var meanCount = Count(ValidationStatuses.Verified) + Count(ValidationStatuses.Unverified);
        double? mean = meanCount == 0
            ? null
            : Math.Round((Sum(ValidationStatuses.Verified) + Sum(ValidationStatuses.Unverified)) / (double)meanCount, 1);

        return new ReportsStatsDto(
            earthquakeUid,
            rows.Sum(x => x.Count),
            Count(ValidationStatuses.Verified),
            Count(ValidationStatuses.Outlier),
            Count(ValidationStatuses.Unverified),
            Count(ValidationStatuses.Pending),
            mean);
    }
}