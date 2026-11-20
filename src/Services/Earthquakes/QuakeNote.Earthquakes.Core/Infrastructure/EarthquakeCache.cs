using QuakeNote.Earthquakes.Core.Dtos;
using QuakeNote.Shared.Cache;
using QuakeNote.Shared.Contracts.Dtos;

namespace QuakeNote.Earthquakes.Core.Infrastructure;

public class EarthquakeCache(ICacheService cache)
{
    public Task<PagedResultDto<EarthquakeListItemDto>> GetListAsync(
        string filterHash, Func<Task<PagedResultDto<EarthquakeListItemDto>>> factory)
        => cache.GetOrCreateAsync(CacheKeys.EarthquakesList(filterHash), CacheTtl.EarthquakesList, factory);

    public Task<EarthquakeDetailDto> GetDetailAsync(Guid uid, Func<Task<EarthquakeDetailDto>> factory)
        => cache.GetOrCreateAsync(CacheKeys.EarthquakeDetail(uid), CacheTtl.EarthquakeDetail, factory);

    /// <summary>Сбрасывает все закэшированные страницы списка.</summary>
    public Task InvalidateListAsync() => cache.RemoveByPrefixAsync(CacheKeys.EarthquakesListPrefix);

    public Task InvalidateDetailAsync(Guid uid) => cache.RemoveAsync(CacheKeys.EarthquakeDetail(uid));

    public async Task InvalidateDetailsAsync(IEnumerable<Guid> uids)
    {
        foreach (var uid in uids.Distinct())
            await cache.RemoveAsync(CacheKeys.EarthquakeDetail(uid));
    }
}