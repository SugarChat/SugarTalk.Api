using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SugarTalk.Core.Masstransit.Consumers;
using SugarTalk.Core.Settings.RabbitMq;

namespace SugarTalk.Core.Masstransit;

public static class MasstransitDependency
{
    public static void RegisterMultiBus(this ContainerBuilder builder, IConfiguration configuration, params Assembly[] assemblies)
    {
        if (assemblies == null || !assemblies.Any())
        {
            throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan.");
        }

        var services = new ServiceCollection();
        
        var scanTypes = assemblies.SelectMany(a => a.GetTypes());
       
        var rabbitMqSettings = new RabbitMqSettings(configuration);

        if (rabbitMqSettings.FoundationBusIsEnabled)
        {
            services.AddMassTransit<IFoundationBus>(x =>
            {
                var foundationEventConsumers = scanTypes
                    .Where(type => type.IsClass && IsAssignableToGenericType(type, typeof(IFoundationEventConsumer<>)))
                    .ToList();

                foreach (var foundationEventConsumer in foundationEventConsumers)
                {
                    x.AddConsumer(foundationEventConsumer);
                }

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqSettings.FoundationBusHost, "/", h =>
                    {
                        h.Username(rabbitMqSettings.FoundationBusUserName);
                        h.Password(rabbitMqSettings.FoundationBusPassword);
                    });

                    cfg.ReceiveEndpoint(rabbitMqSettings.FoundationBusQueueName, e =>
                    {
                        foreach (var foundationEventConsumer in foundationEventConsumers)
                        {
                            e.ConfigureConsumer(context, foundationEventConsumer);
                        }
                    });
                });
            });
        }
        
        if (services.Any())
        {
            builder.Populate(services);
        }
    }

    private static bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();
        if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
        {
            return true;
        }
        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
        {
            return true;
        }
        var baseType = givenType.BaseType;
        return baseType != null && IsAssignableToGenericType(baseType, genericType);
    }
}