using System.Threading.Tasks;
using Hangfire;
using Mediator.Net;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Jobs;

public class SchedulingUpdateMeetingFileTranscriptionStatusRecurringJob : IRecurringJob
{
    private readonly IMediator _mediator;

    public SchedulingUpdateMeetingFileTranscriptionStatusRecurringJob(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task Execute()
    {
        await _mediator.SendAsync(new UpdateMeetingFileTranscriptionStatusCommand()).ConfigureAwait(false);
    }

    public string JobId => nameof(SchedulingUpdateMeetingFileTranscriptionStatusRecurringJob);

    // public string CronExpression => "*/3 * * * *";
    public string CronExpression => "* * * * *";
}