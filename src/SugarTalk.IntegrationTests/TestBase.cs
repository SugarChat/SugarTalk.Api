using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using StackExchange.Redis;
using SugarTalk.Core.Services.Utils;
using SugarTalk.IntegrationTests.Utils.Account;
using Xunit;

namespace SugarTalk.IntegrationTests;

public partial class TestBase : TestUtilBase, IAsyncLifetime, IDisposable
{
    private readonly string _testTopic;
    private readonly string _databaseName;
    private readonly int _redisDatabaseIndex;

    private readonly IdentityUtil _identityUtil;

    private static readonly ConcurrentDictionary<string, IContainer> Containers = new();

    private static readonly ConcurrentDictionary<string, bool> ShouldRunDbUpDatabases = new();

    private static readonly ConcurrentDictionary<int, ConnectionMultiplexer> RedisPool = new();

    protected ILifetimeScope CurrentScope { get; }

    protected IConfiguration CurrentConfiguration => CurrentScope.Resolve<IConfiguration>();

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

        _identityUtil = new IdentityUtil(CurrentScope);
    }
    
    protected IClock MockClock(ContainerBuilder builder, DateTimeOffset? mockedDate = null)
    {
        mockedDate ??= DateTimeOffset.Now;
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(mockedDate.Value);
        builder.Register(_ => clock);
        return clock;
    }
}