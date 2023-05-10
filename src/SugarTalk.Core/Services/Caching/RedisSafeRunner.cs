using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Serilog;
using StackExchange.Redis;
using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.Caching;

public interface IRedisSafeRunner : IScopedDependency
{
    Task ExecuteAsync(Func<ConnectionMultiplexer, Task> func);

    Task<T> ExecuteAsync<T>(Func<ConnectionMultiplexer, Task<T>> func) where T : class;

    Task<List<T>> ExecuteAsync<T>(Func<ConnectionMultiplexer, Task<List<T>>> func) where T : class;

    Task ExecuteWithLockAsync(
        string lockKey, Func<Task> logic, TimeSpan? expiry = null, TimeSpan? wait = null, TimeSpan? retry = null);

    Task<T> ExecuteWithLockAsync<T>(
        string lockKey, Func<Task<T>> logic, TimeSpan? expiry = null, TimeSpan? wait = null, TimeSpan? retry = null) where T : class;
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

    public async Task<T> ExecuteAsync<T>(Func<ConnectionMultiplexer, Task<T>> func) where T : class
    {
        try
        {
            return await func(_redis).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogRedisException(ex);
            return default;
        }
    }

    public async Task<List<T>> ExecuteAsync<T>(Func<ConnectionMultiplexer, Task<List<T>>> func) where T : class
    {
        try
        {
            return await func(_redis).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogRedisException(ex);
            return new List<T>();
        }
    }

    public async Task ExecuteWithLockAsync(
        string lockKey, Func<Task> logic, TimeSpan? expiry = null, TimeSpan? wait = null, TimeSpan? retry = null)
    {
        try
        {
            var redLock = await CreateLockAsync(lockKey, expiry, wait, retry).ConfigureAwait(false);

            await using (redLock)
            {
                // make sure we got the lock
                if (redLock.IsAcquired)
                    await logic();
            }
        }
        catch (Exception ex)
        {
            LogRedisException(ex);
        }
    }

    public async Task<T> ExecuteWithLockAsync<T>(
        string lockKey, Func<Task<T>> logic, TimeSpan? expiry = null, TimeSpan? wait = null, TimeSpan? retry = null) where T : class
    {
        try
        {
            var redLock = await CreateLockAsync(lockKey, expiry, wait, retry).ConfigureAwait(false);

            await using (redLock)
            {
                // make sure we got the lock
                if (redLock.IsAcquired)
                    return await logic();
            }

            return default;
        }
        catch (Exception ex)
        {
            LogRedisException(ex);
            return default;
        }
    }

    private async Task<IRedLock> CreateLockAsync(
        string lockKey, TimeSpan? expiry = null, TimeSpan? wait = null, TimeSpan? retry = null)
    {
        var multiplexers = new List<RedLockMultiplexer> { _redis };
        var redLockFactory = RedLockFactory.Create(multiplexers);

        var expiryTime = expiry ?? TimeSpan.FromSeconds(30);
        var waitTime = wait ?? TimeSpan.FromSeconds(10);
        var retryTime = retry ?? TimeSpan.FromSeconds(1);

        IRedLock redLock;

        if (wait.HasValue && retry.HasValue)
            redLock = await redLockFactory.CreateLockAsync(lockKey, expiryTime, waitTime, retryTime).ConfigureAwait(false);
        else
            redLock = await redLockFactory.CreateLockAsync(lockKey, expiryTime).ConfigureAwait(false);

        return redLock;
    }

    private static void LogRedisException(Exception ex)
    {
        Log.Error(ex, "Redis occur error: {ErrorMessage}", ex.Message);
    }
}