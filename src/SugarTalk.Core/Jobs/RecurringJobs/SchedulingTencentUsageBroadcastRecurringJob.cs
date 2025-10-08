using System;
using System.Threading.Tasks;
using Mediator.Net;
using SugarTalk.Messages.Commands.Tencent;

namespace SugarTalk.Core.Jobs.RecurringJobs;

public class SchedulingTencentUsageBroadcastRecurringJob : IRecurringJob
{
    private readonly IMediator _mediator;

    public SchedulingTencentUsageBroadcastRecurringJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute()
    {
        await _mediator.SendAsync(new TencentUsageBroadcastCommand()).ConfigureAwait(false);
    }

    public string JobId => nameof(SchedulingTencentUsageBroadcastRecurringJob);

    public string CronExpression => "1 0 * * *";


    public TimeZoneInfo TimeZone => TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
}