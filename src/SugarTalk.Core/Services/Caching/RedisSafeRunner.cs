using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using StackExchange.Redis;
using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.Caching;

public interface IRedisSafeRunner : IScopedDependency
{
    Task ExecuteAsync(Func<ConnectionMultiplexer, Task> func);

    Task<T> ExecuteAsync<T>(Func<ConnectionMultiplexer, Task<T>> func) where T : class;

    Task<List<T>> ExecuteAsync<T>(Func<ConnectionMultiplexer, Task<List<T>>> func) where T : class;

    Task ExecuteWithLockAsync(string lockKey, Func<Task> logic,
        TimeSpan? expiry = null, TimeSpan? wait = null, TimeSpan? retry = null);
    
    Task<T> ExecuteWithLockAsync<T>(string lockKey, Func<Task<T>> logic,
        TimeSpan? expiry = null, TimeSpan? wait = null, TimeSpan? retry = null) where T : class;
}

public class RedisSafeRunner : IRedisSafeRunner
{
    private readonly ConnectionMultiplexer _redis;

    public RedisSafeRunner(ConnectionMultiplexer redis)
    {
        _redis = redis;
    }


    public async Task ExecuteAsync(Func<ConnectionMultiplexer, Task> func)
    {
        try
        {
            await func(_redis).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogRedisException(ex);
        }
    }

    public Task<T> ExecuteAsync<T>(Func<ConnectionMultiplexer, Task<T>> func) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> ExecuteAsync<T>(Func<ConnectionMultiplexer, Task<List<T>>> func) where T : class
    {
        throw new NotImplementedException();
    }

    public Task ExecuteWithLockAsync(string lockKey, Func<Task> logic, TimeSpan? expiry = null, TimeSpan? wait = null,
        TimeSpan? retry = null)
    {
        throw new NotImplementedException();
    }

    public Task<T> ExecuteWithLockAsync<T>(string lockKey, Func<Task<T>> logic, TimeSpan? expiry = null, TimeSpan? wait = null,
        TimeSpan? retry = null) where T : class
    {
        throw new NotImplementedException();
    }
    
    private static void LogRedisException(Exception ex)
    {
        Log.Error(ex, "Redis occur error: {ErrorMessage}", ex.Message);
    }
}