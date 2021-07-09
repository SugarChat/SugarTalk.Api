using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using SugarTalk.Core.Settings;

namespace SugarTalk.Api
{
    public class Program
    {
        private static readonly string AppName = typeof(Program).Namespace;
        
        public static int Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var seqSettings = configuration.GetSection(nameof(SeqSettings)).Get<SeqSettings>();
            
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "SugarTalk.Api")
                .WriteTo.Seq(seqSettings.ServerUrl, apiKey: seqSettings.ApiKey)
                .CreateLogger();
            
            try
            {
                Log.Information("Configuring web host ({ApplicationContext})...", AppName);
                var webHost = CreateHostBuilder(args).Build();

                Log.Information("Starting web host ({ApplicationContext})...", AppName);
                webHost.Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}