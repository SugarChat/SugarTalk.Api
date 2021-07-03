using Kurento.NET;
using Mediator.Net;
using Mediator.Net.MicrosoftDependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SugarTalk.Core.Data;
using SugarTalk.Core.Middlewares;
using SugarTalk.Core.Services.Kurento;
using SugarTalk.Core.Settings;

namespace SugarTalk.Core
{
    public static class SugarTalkModule
    {
        public static IServiceCollection LoadSugarTalkModule(this IServiceCollection services)
        {
            services.AddSingleton<IDatabaseProvider, InMemoryDatabaseProvider>();
            services.AddAutoMapper(typeof(SugarTalkModule).Assembly);
            
            services.AddSingleton(p => new KurentoClient("ws://54.183.208.235:8888/kurento"));
            services.AddSingleton<RoomSessionManager>();

            return services;
        }

        public static IServiceCollection LoadSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));

            return services;
        }

        public static IServiceCollection LoadMongoDb(this IServiceCollection services)
        {
            services.AddSingleton(serviceProvider =>
            {
                var settings = serviceProvider.GetService<IOptions<MongoDbSettings>>();
                
                return new MongoClient(settings?.Value.ConnectionString).GetDatabase(settings?.Value.DatabaseName);
            });
            
            services.AddScoped<IRepository, MongoDbRepository>();

            return services;
        }

        public static IServiceCollection LoadMediator(this IServiceCollection services)
        {
            var mediaBuilder = new MediatorBuilder();
            mediaBuilder.RegisterHandlers(typeof(SugarTalkModule).Assembly);
            mediaBuilder.ConfigureGlobalReceivePipe(x => x.UseUnifyResponseMiddleware());
            services.RegisterMediator(mediaBuilder);
            return services;
        }
    }
}