using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.RabbitMq;

public class RabbitMqSettings : IConfigurationSetting
{
    public RabbitMqSettings(IConfiguration configuration)
    {
        FoundationBusIsEnabled = configuration.GetValue<bool>("RabbitMq:FoundationBus:IsEnabled");
        FoundationBusHost = configuration.GetValue<string>("RabbitMq:FoundationBus:Host");
        FoundationBusQueueName = configuration.GetValue<string>("RabbitMq:FoundationBus:QueueName");
        FoundationBusUserName = configuration.GetValue<string>("RabbitMq:FoundationBus:Username");
        FoundationBusPassword = configuration.GetValue<string>("RabbitMq:FoundationBus:Password");
    }
    
    public bool FoundationBusIsEnabled { get; set; }
    
    public string FoundationBusHost { get; }
    
    public string FoundationBusQueueName { get; }
    
    public string FoundationBusUserName { get; }
    
    public string FoundationBusPassword { get; }
}