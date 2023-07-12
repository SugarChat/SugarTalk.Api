using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.SignalR;
using SugarTalk.Core.Hubs;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting;

public class ScreenSharedEventHandler : IEventHandler<ScreenSharedEvent>
{
    private readonly IHubContext<MeetingHub> _meetingHub;
    private readonly IMeetingDataProvider _meetingDataProvider;

    public ScreenSharedEventHandler(IHubContext<MeetingHub> meetingHub, IMeetingDataProvider meetingDataProvider)
    {
        _meetingHub = meetingHub;
        _meetingDataProvider = meetingDataProvider;
    }

    public async Task Handle(IReceiveContext<ScreenSharedEvent> context, CancellationToken cancellationToken)
    {
        if (context.Message?.MeetingUserSession == null) return;
        
        var meeting = await _meetingDataProvider
            .GetMeetingByIdAsync(context.Message.MeetingUserSession.MeetingId, cancellationToken).ConfigureAwait(false);

        await _meetingHub.Clients
            .GroupExcept(meeting.MeetingNumber, context.Message.MeetingUserSession.UserSessionStreams.FirstOrDefault().StreamId)
            .SendAsync("OtherScreenShared", context.Message.MeetingUserSession, cancellationToken).ConfigureAwait(false);
    }
}