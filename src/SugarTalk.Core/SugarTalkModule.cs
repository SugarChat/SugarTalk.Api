using Kurento.NET;
using Mediator.Net;
using Mediator.Net.MicrosoftDependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SugarTalk.Core.Data;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Middlewares;
using SugarTalk.Core.Services.Authentication;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Services.Users;
using SugarTalk.Core.Settings;

namespace SugarTalk.Core
{
    public static class SugarTalkModule
    {
        public static IServiceCollection LoadSugarTalkModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.LoadSettings(configuration)
                .AddAutoMapper(typeof(SugarTalkModule).Assembly)
                .LoadMongoDb()
                .LoadMediator()
                .LoadServices();
            
            return services;
        }

        private static IServiceCollection LoadSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));
            services.Configure<GoogleSettings>(configuration.GetSection(nameof(GoogleSettings)));
            services.Configure<FacebookSettings>(configuration.GetSection(nameof(FacebookSettings)));
            services.Configure<WebRtcIceServerSettings>(configuration.GetSection(nameof(WebRtcIceServerSettings)));
            
            return services;
        }

        private static IServiceCollection LoadMongoDb(this IServiceCollection services)
        {
            services.AddSingleton(serviceProvider =>
            {
                var settings = serviceProvider.GetService<IOptions<MongoDbSettings>>();
                return new MongoClient(settings?.Value.ConnectionString).GetDatabase(settings?.Value.DatabaseName);
            });
            
            services.AddScoped<IRepository, MongoDbRepository>();
            services.AddScoped<IMongoDbRepository, MongoDbRepository>();
            
            return services;
        }

        private static IServiceCollection LoadMediator(this IServiceCollection services)
        {
            var mediaBuilder = new MediatorBuilder();
            mediaBuilder.RegisterHandlers(typeof(SugarTalkModule).Assembly);
            mediaBuilder.ConfigureGlobalReceivePipe(x => x.UseUnifyResponseMiddleware());
            services.RegisterMediator(mediaBuilder);
            return services;
        }
        
        private static IServiceCollection LoadServices(this IServiceCollection services)
        {
            services.AddScoped<IMeetingDataProvider, MeetingDataProvider>();
            services.AddScoped<IMeetingSessionDataProvider, MeetingSessionDataProvider>();
            services.AddScoped<IMeetingSessionService, MeetingSessionService>();
            services.AddScoped<IMeetingService, MeetingService>();

            services.AddScoped<IUserSessionDataProvider, UserSessionDataProvider>();
            services.AddScoped<IUserSessionService, UserSessionService>();
            services.AddScoped<IUserDataProvider, UserDataProvider>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<ITokenDataProvider, TokenDataProvider>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            
            return services;
        }
    }
}