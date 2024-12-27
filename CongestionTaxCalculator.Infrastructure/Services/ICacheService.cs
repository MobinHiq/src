namespace CongestionTaxCalculator.Infrastructure.Services;

public interface ICacheService
{
    Task<T> GetOrSetAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);
            
    Task RemoveAsync(
        string key, 
        CancellationToken cancellationToken = default);
}