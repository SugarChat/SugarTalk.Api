﻿using System;
using System.Linq;
using System.Reflection;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Google.Cloud.Translation.V2;
using Mediator.Net;
using Mediator.Net.Autofac;
using Mediator.Net.Middlewares.Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using SugarTalk.Core.Data;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Masstransit;
using SugarTalk.Core.Middlewares;
using SugarTalk.Core.Middlewares.FluentMessageValidator;
using SugarTalk.Core.Middlewares.UnifyResponse;
using SugarTalk.Core.Middlewares.UnitOfWork;
using SugarTalk.Core.Services.Caching;
using SugarTalk.Core.Settings;
using SugarTalk.Core.Settings.Google;
using Module = Autofac.Module;

namespace SugarTalk.Core
{
    public class SugarTalkModule : Module
    {
        private readonly ILogger _logger;
        private readonly Assembly[] _assemblies;
        private readonly IConfiguration _configuration;

        public SugarTalkModule(ILogger logger, IConfiguration configuration, params Assembly[] assemblies)
        {
            _logger = logger;
            _assemblies = assemblies;
            _configuration = configuration;

            if (_logger == null)
                throw new ArgumentException(nameof(_logger));

            if (_assemblies == null || !_assemblies.Any())
                throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan.");
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterLogger(builder);
            RegisterSettings(builder);
            RegisterMediator(builder);
            RegisterCaching(builder);
            RegisterDatabase(builder);
            RegisterDependency(builder);
            RegisterAutoMapper(builder);
            RegisterTranslationClient(builder);
            RegisterMultiBus(builder, _configuration);
        }

        private void RegisterMultiBus(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterMultiBus(configuration, _assemblies);
        }

        private void RegisterLogger(ContainerBuilder builder)
        {
            builder.RegisterInstance(_logger).AsSelf().AsImplementedInterfaces().SingleInstance();
        }

        private void RegisterSettings(ContainerBuilder builder)
        {
            var settingTypes = typeof(SugarTalkModule).Assembly.GetTypes()
                .Where(t => t.IsClass && typeof(IConfigurationSetting).IsAssignableFrom(t))
                .ToArray();

            builder.RegisterTypes(settingTypes).AsSelf().SingleInstance();
        }

        private void RegisterMediator(ContainerBuilder builder)
        {
            var mediatorBuilder = new MediatorBuilder();
            mediatorBuilder.RegisterHandlers(_assemblies);
            mediatorBuilder.ConfigureGlobalReceivePipe(c =>
            {
                c.UseUnitOfWork();
                c.UseUnifyResponse();
                c.UseMessageValidator();
                c.UseSerilog(logger: _logger);
            });
            builder.RegisterMediator(mediatorBuilder);
        }
        
        private void RegisterCaching(ContainerBuilder builder)
        {
            builder.Register(cfx =>
            {
                var pool = cfx.Resolve<IRedisConnectionPool>();
                return pool.GetConnection();
            }).ExternallyOwned();
        }

        private void RegisterDatabase(ContainerBuilder builder)
        {
            builder.RegisterType<SugarTalkDbContext>()
                .AsSelf()
                .As<DbContext>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository>().As<IRepository>().InstancePerLifetimeScope();
        }

        private void RegisterDependency(ContainerBuilder builder)
        {
            foreach (var type in typeof(IDependency).Assembly.GetTypes()
                         .Where(type => type.IsClass && typeof(IDependency).IsAssignableFrom(type)))
            {
                if (typeof(IScopedDependency).IsAssignableFrom(type))
                    builder.RegisterType(type).AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
                else if (typeof(ISingletonDependency).IsAssignableFrom(type))
                    builder.RegisterType(type).AsSelf().AsImplementedInterfaces().SingleInstance();
                else if (typeof(ITransientDependency).IsAssignableFrom(type))
                    builder.RegisterType(type).AsSelf().AsImplementedInterfaces().InstancePerDependency();
                else
                    builder.RegisterType(type).AsSelf().AsImplementedInterfaces();
            }
        }
        
        private void RegisterTranslationClient(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var googleTranslateApiKey = c.Resolve<GoogleTranslateApiKeySetting>().Value;
                return TranslationClient.CreateFromApiKey(googleTranslateApiKey);
            }).AsSelf().InstancePerLifetimeScope();
        }

        private void RegisterAutoMapper(ContainerBuilder builder)
        {
            builder.RegisterAutoMapper(typeof(SugarTalkModule).Assembly);
        }
    }
}