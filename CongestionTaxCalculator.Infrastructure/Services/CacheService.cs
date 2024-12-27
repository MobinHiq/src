using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CongestionTaxCalculator.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IMemoryCache cache,
        ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.SlidingExpiration = expiration ?? TimeSpan.FromHours(1);
                return await factory();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache error for key: {Key}", key);
            return await factory();
        }
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}