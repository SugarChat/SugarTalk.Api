using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace SugarTalk.Core.Services.Caching;

public class MemoryCacheService : ICachingService
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    
    public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        _memoryCache.TryGetValue<T>(key, out var result);

        return Task.FromResult(result);
    }

    public Task SetAsync(string key, object data, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        if (expiry == null)
            _memoryCache.Set(key, data);
        else
            _memoryCache.Set(key, data, expiry.Value);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);

        return Task.CompletedTask;
    }
}