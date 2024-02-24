using System.Threading.Tasks;
using Hangfire;
using Mediator.Net;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Jobs;

public class SchedulingGetMeetingTranscriptionUrlStatusRecurringJob : IRecurringJob
{
    private readonly IMediator _mediator;

    public SchedulingGetMeetingTranscriptionUrlStatusRecurringJob(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task Execute()
    {
        await _mediator.SendAsync(new SchedulingGetMeetingTranscriptionUrlStatusCommand()).ConfigureAwait(false);
    }

    public string JobId => nameof(SchedulingGetMeetingTranscriptionUrlStatusRecurringJob);

    public string CronExpression => "*/5 * * * *";
}