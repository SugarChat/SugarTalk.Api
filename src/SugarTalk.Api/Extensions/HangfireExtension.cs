using Serilog;
using Hangfire;
using Newtonsoft.Json;
using Hangfire.Correlate;
using SugarTalk.Core.Constants;
using SugarTalk.Core.Jobs;
using SugarTalk.Core.Services.Jobs;
using SugarTalk.Core.Settings.Caching;

namespace SugarTalk.Api.Extensions;

public static class HangfireExtension
{
    public static void AddHangfireInternal(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire((sp, c) =>
        {
            c.UseCorrelate(sp);
            c.UseMaxArgumentSizeToRender(int.MaxValue);
            c.UseFilter(new AutomaticRetryAttribute { Attempts = 0 });
            c.UseRedisStorage(new RedisCacheConnectionStringSetting(configuration).Value);
            c.UseSerializerSettings(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        });
        
        services.AddHangfireServer(opt =>
        {
            opt.ServerTimeout = TimeSpan.FromHours(2);
            opt.Queues = new[]
            {
                HangfireConstants.DefaultQueue,
                HangfireConstants.MeetingSummaryQueue
            };
        });
    }

    public static void UseHangfireInternal(this IApplicationBuilder app)
    {
        app.UseHangfireDashboard(options: new DashboardOptions
        {
            IgnoreAntiforgeryToken = true
        });

        app.ScanHangfireRecurringJobs();
    }
    
    public static void ScanHangfireRecurringJobs(this IApplicationBuilder app)
    {
        var backgroundJobClient = app.ApplicationServices.GetRequiredService<ISugarTalkBackgroundJobClient>();
        
        var recurringJobTypes = typeof(IRecurringJob).Assembly.GetTypes().Where(type => type.IsClass && typeof(IRecurringJob).IsAssignableFrom(type)).ToList();
        
        foreach (var type in recurringJobTypes)
        {
            var job = (IRecurringJob) app.ApplicationServices.GetRequiredService(type);

            if (string.IsNullOrEmpty(job.CronExpression))
            {
                Log.Error("Recurring Job Cron Expression Empty, {Job}", job.GetType().FullName);
                continue;
            }

            backgroundJobClient.AddOrUpdateRecurringJob<IJobSafeRunner>(job.JobId, r => r.Run(job.JobId, type), job.CronExpression, job.TimeZone);
        }
    }
}