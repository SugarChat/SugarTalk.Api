using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Caching;

public class RedisCacheConnectionStringSetting : IConfigurationSetting<string>
{
    public RedisCacheConnectionStringSetting(IConfiguration configuration)
    {
        Value = configuration.GetValue<string>("RedisCacheConnectionString");
    }
    
    public string Value { get; set; }
}

public class RedisServerUrlSetting : IConfigurationSetting<string>
{
    public RedisServerUrlSetting(IConfiguration configuration)
    {
        Value = configuration.GetValue<string>("RedisServerUrl");
    }
    
    public string Value { get; set; }
}

public class RedisPortSetting : IConfigurationSetting<string>
{
    public RedisPortSetting(IConfiguration configuration)
    {
        Value = configuration.GetValue<string>("RedisPort");
    }
    
    public string Value { get; set; }
}

public class RedisPasswordSetting : IConfigurationSetting<string>
{
    public RedisPasswordSetting(IConfiguration configuration)
    {
        Value = configuration.GetValue<string>("RedisPassword");
    }
    
    public string Value { get; set; }
}

public class RedisUseSslSetting : IConfigurationSetting<string>
{
    public RedisUseSslSetting(IConfiguration configuration)
    {
        Value = configuration.GetValue<string>("RedisUseSsl");
    }
    
    public string Value { get; set; }
}

public class RedisDbNumberSetting : IConfigurationSetting<string>
{
    public RedisDbNumberSetting(IConfiguration configuration)
    {
        Value = configuration.GetValue<string>("RedisDbNumber");
    }
    
    public string Value { get; set; }
}