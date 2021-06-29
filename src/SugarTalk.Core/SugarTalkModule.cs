using System;
using System.Reflection;
using Kurento.NET;
using Microsoft.Extensions.DependencyInjection;
using SugarTalk.Core.Services.Kurento;

namespace SugarTalk.Core
{
    public static class SugarTalkModule
    {
        public static void LoadSugarTalkModule(this IServiceCollection services)
        {
            services.AddSingleton<IDatabaseProvider, InMemoryDatabaseProvider>();
            services.AddAutoMapper(typeof(SugarTalkModule).Assembly);
            
            services.AddSingleton(p => new KurentoClient("ws://54.183.208.235:8888/kurento"));
            services.AddSingleton<RoomSessionManager>();
            
            
        }
    }
}