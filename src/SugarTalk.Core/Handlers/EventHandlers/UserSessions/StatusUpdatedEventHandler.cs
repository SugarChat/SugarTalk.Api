using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.SignalR;
using SugarTalk.Core.Hubs;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Events.UserSessions;

namespace SugarTalk.Core.Handlers.EventHandlers.UserSessions
{
    public class StatusUpdatedEventHandler : IEventHandler<StatusUpdatedEvent>
    {
        private readonly IHubContext<MeetingHub> _meetingHub;
        private readonly IMeetingSessionDataProvider _meetingSessionDataProvider;
        public StatusUpdatedEventHandler(IHubContext<MeetingHub> meetingHub, IMeetingSessionDataProvider meetingSessionDataProvider)
        {
            _meetingHub = meetingHub;
            _meetingSessionDataProvider = meetingSessionDataProvider;
        }

        public async Task Handle(IReceiveContext<StatusUpdatedEvent> context, CancellationToken cancellationToken)
        {
            var meetingSession = await _meetingSessionDataProvider
                .GetMeetingSessionById(context.Message.UserSession.MeetingSessionId, cancellationToken)
                .ConfigureAwait(false);
            
            await _meetingHub.Clients
                .GroupExcept(meetingSession.MeetingNumber, context.Message.UserSession.ConnectionId)
                .SendAsync("OtherUserSessionStatusChanged", context.Message.UserSession, cancellationToken).ConfigureAwait(false);
        }
    }
}