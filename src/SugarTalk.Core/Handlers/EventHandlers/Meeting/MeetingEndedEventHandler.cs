using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting;

public class MeetingEndedEventHandler : IEventHandler<MeetingEndedEvent>
{
    private readonly IMeetingDataProvider _meetingDataProvider;

    public MeetingEndedEventHandler(IMeetingDataProvider meetingDataProvider)
    {
        _meetingDataProvider = meetingDataProvider;
    }

    public async Task Handle(IReceiveContext<MeetingEndedEvent> context, CancellationToken cancellationToken)
    {
        await _meetingDataProvider.CompleteMeetingByMeetingNumberAsync(context.Message.MeetingNumber,
            cancellationToken).ConfigureAwait(false);
        var sessions =
            await _meetingDataProvider.GetOnlineMeetingUserSessionsAsync(context.Message.MeetingUserSessionIds,
                cancellationToken).ConfigureAwait(false);
        await _meetingDataProvider.UpdateMeetingUserSessionsOnlineStatusAsync(sessions,
            MeetingUserSessionOnlineType.OutMeeting, cancellationToken).ConfigureAwait(false);
    }
}