using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SugarTalk.Core;

namespace SugarTalk.Tests
{
    public class TestBase : IDisposable
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IServiceCollection _serviceCollection;

        protected TestBase()
        {
            _configuration = LoadConfiguration();
            _serviceCollection = new ServiceCollection();

            RegisterSugarTalkModule(_serviceCollection);
            RegisterHttpContextAccessor(_serviceCollection);
        }

        private IConfigurationRoot LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }
        
        private void RegisterSugarTalkModule(IServiceCollection services)
        {
            services.LoadSugarTalkModule(_configuration);
        }

        private void RegisterHttpContextAccessor(IServiceCollection services)
        {
            var httpContextAccessor = new HttpContextAccessor {HttpContext = new DefaultHttpContext()};
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
        }
        
        protected async Task Run<T>(Func<T, Task> action)
        {
            await action(_serviceCollection.BuildServiceProvider().GetService<T>());
        }
        
        public void Dispose()
        {
            _serviceCollection.Clear();
        }
    }
}