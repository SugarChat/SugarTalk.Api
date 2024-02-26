using System.Threading.Tasks;
using Hangfire;
using Mediator.Net;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Jobs.RecurringJobs;

public class SchedulingCheckAppointmentMeetingDateRecurringJob : IRecurringJob
{
    private readonly IMediator _mediator;

    public SchedulingCheckAppointmentMeetingDateRecurringJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute()
    {
        await _mediator.SendAsync(new CheckAppointmentMeetingDateCommand()).ConfigureAwait(false);
    }

    public string JobId => nameof(SchedulingCheckAppointmentMeetingDateRecurringJob);

    public string CronExpression => Cron.Minutely();
}