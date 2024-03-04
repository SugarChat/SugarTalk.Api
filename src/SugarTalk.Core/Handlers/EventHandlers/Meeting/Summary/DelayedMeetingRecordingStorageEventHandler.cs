using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Jobs;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Events.Meeting.Summary;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting.Summary;

public class DelayedMeetingRecordingStorageEventHandler : IEventHandler<DelayedMeetingRecordingStorageEvent>
{
    private readonly ISugarTalkBackgroundJobClient _sugarTalkBackgroundJobClient;

    public DelayedMeetingRecordingStorageEventHandler( ISugarTalkBackgroundJobClient sugarTalkBackgroundJobClient)
    {
        _sugarTalkBackgroundJobClient = sugarTalkBackgroundJobClient;
    }

    public Task Handle(IReceiveContext<DelayedMeetingRecordingStorageEvent> context, CancellationToken cancellationToken)
    {
        _sugarTalkBackgroundJobClient.Enqueue<IMeetingService>(x =>
            x.DelayStorageMeetingRecordVideoJobAsync(context.Message.EgressId, context.Message.MeetingRecordId, context.Message.Token, cancellationToken).ConfigureAwait(false));

        return Task.CompletedTask;
    }
}