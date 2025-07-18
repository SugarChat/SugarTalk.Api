using System;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Enums.Caching;

namespace SugarTalk.Core.Services.Caching;

public interface ICacheManager : IScopedDependency
{
    Task<T> GetAsync<T>(string key, CachingType cachingType, CancellationToken cancellationToken) where T : class;
    
    Task SetAsync(string key, object data, CachingType cachingType, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    
    Task<T> UsingCacheAsync<T>(
        string key, Func<string, Task<T>> whenNotFound, 
        TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;

    Task<T> GetOrAddAsync<T>(
        string key, Func<string, Task<T>> whenNotFound, CachingType cachingType,
        TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
    
    Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> whenNotFound, CachingType cachingType,
        TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
    
    Task RemoveAsync(string key, CachingType cachingType, CancellationToken cancellationToken = default);
}

public class CacheManager : ICacheManager
{
    private readonly RedisCacheService _redisCacheService;
    private readonly MemoryCacheService _memoryCacheService;

    public CacheManager(RedisCacheService redisCacheService, MemoryCacheService memoryCacheService)
    {
        _redisCacheService = redisCacheService;
        _memoryCacheService = memoryCacheService;
    }

    public async Task<T> GetAsync<T>(string key, CachingType cachingType, CancellationToken cancellationToken) where T: class
    {
        var cachingService = GetCachingService(cachingType);

        return await cachingService.GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task SetAsync(string key, object data, CachingType cachingType, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var cachingService = GetCachingService(cachingType);

        await cachingService.SetAsync(key, data, expiry, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<T> UsingCacheAsync<T>(
        string key, Func<string, Task<T>> whenNotFound, 
        TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        var result = await GetAsync<T>(key, CachingType.MemoryCache, cancellationToken).ConfigureAwait(false);
        
        if (result != null)
            return result;

        expiry ??= TimeSpan.FromMinutes(5);
        
        result = await GetOrAddAsync(key, whenNotFound, CachingType.RedisCache, expiry, cancellationToken).ConfigureAwait(false);
        
        await SetAsync(key, result, CachingType.MemoryCache, expiry.Value, cancellationToken).ConfigureAwait(false);
        
        return result;
    }
    
    public async Task<T> GetOrAddAsync<T>(
        string key, Func<string, Task<T>> whenNotFound, CachingType cachingType,
        TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T: class
    {
        var cachedResult = await GetAsync<T>(key, cachingType, cancellationToken).ConfigureAwait(false);

        if (cachedResult != null)
            return cachedResult;

        var result = await whenNotFound(key);

        await SetAsync(key, result, cachingType, expiry, cancellationToken).ConfigureAwait(false);

        return result;
    }

    public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> whenNotFound, 
        CachingType cachingType, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        ICachingService cachingService = cachingType switch
        {
            CachingType.RedisCache => _redisCacheService,
            CachingType.MemoryCache => _memoryCacheService
        };

        var cachedResult = await cachingService.GetAsync<T>(key, cancellationToken).ConfigureAwait(false);

        if (cachedResult != null)
            return cachedResult;

        var result = await whenNotFound();

        await cachingService.SetAsync(key, result, expiry, cancellationToken).ConfigureAwait(false);

        return result;
    }

    private ICachingService GetCachingService(CachingType cachingType)
    {
        return cachingType switch
        {
            CachingType.RedisCache => _redisCacheService,
            CachingType.MemoryCache => _memoryCacheService
        };
    }
    
    public async Task RemoveAsync(string key, CachingType cachingType, CancellationToken cancellationToken = default)
    {
        ICachingService cachingService = cachingType switch
        {
            CachingType.RedisCache => _redisCacheService,
            CachingType.MemoryCache => _memoryCacheService
        };
        
        await cachingService.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
    }
}