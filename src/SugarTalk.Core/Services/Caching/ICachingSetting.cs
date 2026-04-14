using System;
using SugarTalk.Messages.Enums.Caching;

namespace SugarTalk.Core.Services.Caching;

public class ICachingSetting
{
    CachingType CachingType { get; }
    
    TimeSpan? Expiry { get; set;  }
}

public class RedisCachingSetting : ICachingSetting
{
    private RedisServer _redisServer;
    private TimeSpan? _expiry;

    public RedisCachingSetting(RedisServer redisServer = RedisServer.System, TimeSpan? expiry = null)
    {
        _redisServer = redisServer;
        _expiry = expiry;
    }

    public CachingType CachingType => CachingType.RedisCache;

    public RedisServer RedisServer => _redisServer;

    public TimeSpan? Expiry { get => _expiry; set => _expiry = value; }
}

public class MemoryCachingSetting : ICachingSetting
{
    private TimeSpan? _expiry;

    public MemoryCachingSetting(TimeSpan? expiry = null)
    {
        _expiry = expiry;
    }

    public CachingType CachingType => CachingType.MemoryCache;
    
    public TimeSpan? Expiry { get => _expiry; set => _expiry = value; }
}
