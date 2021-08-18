using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SugarTalk.Core;
using SugarTalk.Core.Entities;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages.Enums;
using SugarTalk.Messages.Requests.Users;

namespace SugarTalk.Tests
{
    public class TestBase : IDisposable
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IServiceCollection _serviceCollection;
        private readonly ServiceProvider _serviceProvider;

        protected readonly User DefaultUser;

        protected TestBase(bool shouldLoggedInDefaultUser = true)
        {
            _configuration = LoadConfiguration();
            _serviceCollection = new ServiceCollection();

            RegisterSugarTalkModule(_serviceCollection);
            RegisterHttpContextAccessor(_serviceCollection);

            _serviceProvider = _serviceCollection.BuildServiceProvider();

            DefaultUser = CreateDefaultUser();
            
            if (shouldLoggedInDefaultUser)
                Signin(DefaultUser);
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
        
        protected void Signin(User user)
        {
            Run<IHttpContextAccessor>(accessor =>
            {
                accessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new(ClaimTypes.Name, user.DisplayName),
                    new(ClaimTypes.Email, user.Email),
                    new(SugarTalkClaimType.Picture, user.Picture),
                    new(SugarTalkClaimType.ThirdPartyId, user.ThirdPartyId),
                    new(SugarTalkClaimType.ThirdPartyFrom, user.ThirdPartyFrom.ToString())
                }, user.ThirdPartyFrom.ToString()));
            });

            Run<IUserService>(userService =>
            {
                Task.Run(async () =>
                {
                    var response =
                        await userService.SignInFromThirdParty(new SignInFromThirdPartyRequest(), default);
                    
                    user.Id = response.Data.Id;
                });
            });
        }
        
        protected void Run<T>(Action<T> action)
        {
            action(_serviceProvider.GetService<T>());
        }
        
        protected async Task Run<T>(Func<T, Task> action)
        {
            await action(_serviceProvider.GetService<T>());
        }
        
        private User CreateDefaultUser()
        {
            return new()
            {
                ThirdPartyId = "TestThirdPartyId",
                ThirdPartyFrom = ThirdPartyFrom.Google,
                DisplayName = "TestName",
                Email = "test@email.com",
                Picture = "https://www.sugartalk.com/test-picture.png"
            };
        }

        private void ClearDatabaseRecords()
        {
            Run<IMongoDatabase>(db =>
            {
                using var cursor = db.ListCollectionNames();
                
                while (cursor.MoveNext())
                {
                    foreach (var collectionName in cursor.Current)
                    {
                        db.DropCollection(collectionName);
                    }
                }
            });
        }
        
        public void Dispose()
        {
            ClearDatabaseRecords();
            
            _serviceCollection.Clear();
        }
    }
}