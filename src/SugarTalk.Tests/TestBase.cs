using System;
using System.Threading.Tasks;
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
            _serviceCollection = RegisterServiceCollection();
        }

        private IConfigurationRoot LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }
        
        private IServiceCollection RegisterServiceCollection()
        {
            return new ServiceCollection()
                .LoadSugarTalkModule()
                .LoadSettings(_configuration)
                .LoadMongoDb()
                .LoadMediator();
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