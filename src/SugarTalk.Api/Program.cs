using Autofac;
using Autofac.Extensions.DependencyInjection;
using Destructurama;
using Serilog;
using SugarTalk.Core;
using SugarTalk.Core.DbUp;
using SugarTalk.Core.Settings.Logging;
using SugarTalk.Core.Settings.System;

namespace SugarTalk.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            
            var serverUrl = new SerilogServerUrlSetting(configuration).Value;
            var application = new SerilogApplicationSetting(configuration).Value;
            
            Log.Logger = new LoggerConfiguration()
                .Destructure.JsonNetTypes()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", application)
                .WriteTo.Console()
                .WriteTo.Seq(serverUrl)
                .CreateLogger();
            
            try
            {
                Log.Information("Configuring api host ({ApplicationContext})...", application);
                
                new DbUpRunner(new SugarTalkConnectionString(configuration).Value).Run();
                
                var webHost = CreateHostBuilder(args, configuration).Build();

                Log.Information("Starting api host ({ApplicationContext})...", application);
                
                webHost.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", application);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureLogging(l => l.AddSerilog(Log.Logger))
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterModule(new SugarTalkModule(Log.Logger, configuration, typeof(SugarTalkModule).Assembly));
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}