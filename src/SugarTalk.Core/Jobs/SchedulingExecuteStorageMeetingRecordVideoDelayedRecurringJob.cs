using System.Threading.Tasks;
using Mediator.Net;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Jobs;

public class SchedulingExecuteStorageMeetingRecordVideoDelayedRecurringJob : IRecurringJob
{
    private readonly IMediator _mediator;

    public SchedulingExecuteStorageMeetingRecordVideoDelayedRecurringJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute()
    {
        await _mediator.SendAsync(new DelayedMeetingRecordingStorageCommand()).ConfigureAwait(false);
    }

    public string JobId => nameof(SchedulingExecuteStorageMeetingRecordVideoDelayedRecurringJob);
    
    public string CronExpression => "*/5 * * * * ?";
}