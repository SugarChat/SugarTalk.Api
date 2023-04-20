using System;
using System.Collections.Generic;
using StackExchange.Redis;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.Caching;

namespace SugarTalk.Core.Services.Caching;

public interface IRedisConnectionPool : ISingletonDependency
{
    ConnectionMultiplexer GetConnection();
}

public class RedisConnectionPool : IRedisConnectionPool
{
    private readonly List<ConnectionMultiplexer> _pool;

    public RedisConnectionPool(RedisCacheConnectionStringSetting connectionStringSetting)
    {
        _pool = new List<ConnectionMultiplexer>();

        var connectionString = connectionStringSetting.Value;
        
        var instance1 = ConnectionMultiplexer.Connect(connectionString); 
        var instance2 = ConnectionMultiplexer.Connect(connectionString); 
        var instance3 = ConnectionMultiplexer.Connect(connectionString);
        var instance4 = ConnectionMultiplexer.Connect(connectionString);
        var instance5 = ConnectionMultiplexer.Connect(connectionString);
        var instance6 = ConnectionMultiplexer.Connect(connectionString);
        var instance7 = ConnectionMultiplexer.Connect(connectionString);
        var instance8 = ConnectionMultiplexer.Connect(connectionString);
        var instance9 = ConnectionMultiplexer.Connect(connectionString);
        var instance10 = ConnectionMultiplexer.Connect(connectionString);
        
        _pool.AddRange(new[]
        {
            instance1, instance2, instance3, instance4, instance5, 
            instance6, instance7, instance8, instance9, instance10
        });
    }

    public ConnectionMultiplexer GetConnection()
    {
        var random = new Random();
        var next = random.Next(0, _pool.Count - 1);
        return _pool[next];
    }
}