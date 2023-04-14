using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages;
using SugarTalk.Messages.Enums;
using SugarTalk.Messages.Requests.Users;
using Xunit;

namespace SugarTalk.Tests
{
    public partial class TestBase : TestUtilBase, IAsyncLifetime, IDisposable
    {
        private readonly string _testTopic;
        private readonly string _databaseName;
        private readonly int _redisDatabaseIndex;

        private static readonly ConcurrentDictionary<string, IContainer> Containers = new();

        private static readonly ConcurrentDictionary<string, bool> ShouldRunDbUpDatabases = new();

        private static readonly ConcurrentDictionary<int, ConnectionMultiplexer> RedisPool = new();
        
        protected ILifetimeScope CurrentScope { get; }

        protected IConfiguration CurrentConfiguration => CurrentScope.Resolve<IConfiguration>();

        protected readonly User DefaultUser;

        protected TestBase(string testTopic, string databaseName, int redisDatabaseIndex)
        {
            _testTopic = testTopic;
            _databaseName = databaseName;
            _redisDatabaseIndex = redisDatabaseIndex;
            
            var root = Containers.GetValueOrDefault(testTopic);
            
            if (root == null)
            {
                var containerBuilder = new ContainerBuilder();
                RegisterBaseContainer(containerBuilder);
                root = containerBuilder.Build();
                Containers[testTopic] = root;
            }
            
            CurrentScope = root.BeginLifetimeScope();
            
            RunDbUpIfRequired();
            
            SetupScope(CurrentScope);
            
            DefaultUser = CreateDefaultUser();
            Signin(DefaultUser);
        }

        protected void Signin(User user)
        {
            Run<IHttpContextAccessor>(accessor =>
            {
                accessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new(ClaimTypes.Name, user.DisplayName),
                    new(ClaimTypes.Email, user.Email),
                    new(SugarTalkConstants.Picture, user.Picture),
                    new(SugarTalkConstants.ThirdPartyId, user.ThirdPartyId),
                    new(SugarTalkConstants.ThirdPartyFrom, user.ThirdPartyFrom.ToString())
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
    }
}