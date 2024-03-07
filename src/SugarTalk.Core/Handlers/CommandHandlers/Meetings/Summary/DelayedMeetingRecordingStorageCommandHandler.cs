using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Summary;

public class DelayedMeetingRecordingStorageCommandHandler : ICommandHandler<DelayedMeetingRecordingStorageCommand>
{
    private readonly IMeetingService _meetingService;

    public DelayedMeetingRecordingStorageCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task Handle(IReceiveContext<DelayedMeetingRecordingStorageCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.ExecuteStorageMeetingRecordVideoDelayedJobAsync(context.Message, cancellationToken).ConfigureAwait(false);

        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);
    }
}