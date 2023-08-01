using Hangfire;
using Mediator.Net;
using System.Threading.Tasks;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Jobs.RecurringJobs;

public class ScheduleAutoDeactivateMeetingJob : IRecurringJob
{
    private readonly IMediator _mediator;

    public ScheduleAutoDeactivateMeetingJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute()
    {
        await _mediator.SendAsync(new ScheduleAutoDeactivateMeetingCommand()).ConfigureAwait(false);
    }

    public string JobId => nameof(ScheduleAutoDeactivateMeetingJob);
    
    public string CronExpression => Cron.Minutely();
}