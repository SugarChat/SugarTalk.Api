using System.Threading.Tasks;
using Mediator.Net;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Jobs.RecurringJobs;

public class SchedulingUpdateAppointmentMeetingRecurringJob : IRecurringJob
{
    private readonly IMediator _mediator;

    public SchedulingUpdateAppointmentMeetingRecurringJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute()
    {
        await _mediator.SendAsync(new UpdateRepeatMeetingCommand()).ConfigureAwait(false);
    }

    public string JobId => nameof(SchedulingUpdateAppointmentMeetingRecurringJob);

    // 每天 12：01 执行
    public string CronExpression => "1 0 * * * ";
}