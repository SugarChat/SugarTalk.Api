using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SugarTalk.Core.Services.Caching;

public class RedisCacheService : ICachingService
{
    private readonly IRedisSafeRunner _redisSafeRunner;

    public RedisCacheService(IRedisSafeRunner redisSafeRunner)
    {
        _redisSafeRunner = redisSafeRunner;
    }
    
    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        return await _redisSafeRunner.ExecuteAsync(async redisConnection =>
        {
            var cachedResult = await redisConnection.GetDatabase().StringGetAsync(key).ConfigureAwait(false);
            return !cachedResult.IsNullOrEmpty
                ? typeof(T) == typeof(string) ? cachedResult.ToString() as T :
                    JsonConvert.DeserializeObject<T>(cachedResult)
                : null;
        }).ConfigureAwait(false);
    }

    public async Task SetAsync(string key, object data, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        await _redisSafeRunner.ExecuteAsync(async redisConnection =>
        {
            if (data != null)
            {
                var stringValue = data as string ?? JsonConvert.SerializeObject(data);
                await redisConnection.GetDatabase().StringSetAsync(key, stringValue, expiry).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _redisSafeRunner.ExecuteAsync(async redisConnection =>
        {
            var db = redisConnection.GetDatabase();
            await db.KeyDeleteAsync(key);
        }).ConfigureAwait(false);
    }
}