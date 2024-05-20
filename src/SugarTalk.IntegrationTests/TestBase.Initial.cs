using Autofac;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Cloud.Translation.V2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using SugarTalk.Core;
using MySqlConnector;
using Newtonsoft.Json;
using OpenAI.Interfaces;
using StackExchange.Redis;
using SugarTalk.Core.DbUp;
using SugarTalk.Core.Hubs;
using SugarTalk.Core.Services.Aws;
using SugarTalk.Core.Services.Jobs;
using SugarTalk.Core.Settings.System;
using SugarTalk.Core.Settings.Caching;
using SugarTalk.Core.Services.Identity;
using SugarTalk.IntegrationTests.Mocks;

namespace SugarTalk.IntegrationTests;

public partial class TestBase
{
    private readonly List<string> _tableRecordsDeletionExcludeList = new()
    {
        "schemaversions"
    };

    public async Task InitializeAsync()
    {
        await _identityUtil.CreateUser(new TestCurrentUser());
    }
    
    private void RegisterBaseContainer(ContainerBuilder containerBuilder)
    {
        var logger = Substitute.For<ILogger>();
        
        var configuration = RegisterConfiguration(containerBuilder);
        
        containerBuilder.RegisterModule(
            new SugarTalkModule(logger, configuration, typeof(SugarTalkModule).Assembly, typeof(TestBase).Assembly));

        containerBuilder.RegisterInstance(Substitute.For<TranslationClient>());
        containerBuilder.RegisterInstance(new TestCurrentUser()).As<ICurrentUser>();
        containerBuilder.RegisterInstance(Substitute.For<IMemoryCache>()).AsImplementedInterfaces();
        containerBuilder.RegisterInstance(Substitute.For<IOpenAIService>()).AsImplementedInterfaces();
        containerBuilder.RegisterInstance(Substitute.For<IHttpContextAccessor>()).AsImplementedInterfaces();
        containerBuilder.RegisterInstance(Substitute.For<IHubContext<MeetingHub>>()).AsImplementedInterfaces();
        containerBuilder.RegisterInstance(Substitute.For<IAwsS3Service>()).AsImplementedInterfaces();
        
        RegisterRedis(containerBuilder);
        RegisterSugarTalkBackgroundJobClient(containerBuilder);
    }
    
    private IConfiguration RegisterConfiguration(ContainerBuilder containerBuilder)
    {
        var targetJson = $"appsettings{_testTopic}.json";
        File.Copy("appsettings.json", targetJson, true);
        dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(targetJson));
        jsonObj["ConnectionStrings"]["SugarTalkConnectionString"] =
            jsonObj["ConnectionStrings"]["SugarTalkConnectionString"].ToString()
                .Replace("Database=sugar_talk", $"Database={_databaseName}");
        File.WriteAllText(targetJson, JsonConvert.SerializeObject(jsonObj));
        var configuration = new ConfigurationBuilder().AddJsonFile(targetJson).Build();
        containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();
        return configuration;
    }

    private void RegisterSugarTalkBackgroundJobClient(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<MockingBackgroundJobClient>().As<ISugarTalkBackgroundJobClient>().InstancePerLifetimeScope();
    }
    
    private void RegisterRedis(ContainerBuilder builder)
    {
        builder.Register(cfx =>
        {
            if (RedisPool.ContainsKey(_redisDatabaseIndex))
                return RedisPool[_redisDatabaseIndex];
                
            var redisConnectionSetting = cfx.Resolve<RedisCacheConnectionStringSetting>();
                
            var connString = $"{redisConnectionSetting.Value},defaultDatabase={_redisDatabaseIndex}";

            var instance = ConnectionMultiplexer.Connect(connString);
            
            return RedisPool.GetOrAdd(_redisDatabaseIndex, instance);
            
        }).ExternallyOwned();
    }
    
    private void RunDbUpIfRequired()
    {
        if (!ShouldRunDbUpDatabases.GetValueOrDefault(_databaseName, true))
            return;

        new DbUpRunner(new SugarTalkConnectionString(CurrentConfiguration).Value).Run();

        ShouldRunDbUpDatabases[_databaseName] = false;
    }
    
    private void FlushRedisDatabase()
    {
        try
        {
            if (!RedisPool.TryGetValue(_redisDatabaseIndex, out var redis)) return;
            
            foreach (var endpoint in redis.GetEndPoints())
            {
                var server = redis.GetServer(endpoint);
                    
                server.FlushDatabase(_redisDatabaseIndex);    
            }
        }
        catch
        {
            // ignored
        }
    }
    
    private void ClearDatabaseRecord()
    {
        try
        {
            var connection = new MySqlConnection(new SugarTalkConnectionString(CurrentConfiguration).Value);

            var deleteStatements = new List<string>();

            connection.Open();

            using var reader = new MySqlCommand(
                    $"SELECT table_name FROM INFORMATION_SCHEMA.tables WHERE table_schema = '{_databaseName}';",
                    connection)
                .ExecuteReader();

            deleteStatements.Add($"SET SQL_SAFE_UPDATES = 0");
            while (reader.Read())
            {
                var table = reader.GetString(0);

                if (!_tableRecordsDeletionExcludeList.Contains(table))
                {
                    deleteStatements.Add($"DELETE FROM `{table}`");
                }
            }

            deleteStatements.Add($"SET SQL_SAFE_UPDATES = 1");

            reader.Close();

            var strDeleteStatements = string.Join(";", deleteStatements) + ";";

            new MySqlCommand(strDeleteStatements, connection).ExecuteNonQuery();

            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up data, {_testTopic}, {ex}");
        }
    }
    
    public void Dispose()
    {
        ClearDatabaseRecord();
        FlushRedisDatabase();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}