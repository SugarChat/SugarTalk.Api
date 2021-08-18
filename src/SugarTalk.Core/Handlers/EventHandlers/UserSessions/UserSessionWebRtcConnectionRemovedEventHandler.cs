using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.SignalR;
using SugarTalk.Core.Hubs;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages.Events.UserSessions;

namespace SugarTalk.Core.Handlers.EventHandlers.UserSessions
{
    public class UserSessionWebRtcConnectionRemovedEventHandler : IEventHandler<UserSessionWebRtcConnectionRemovedEvent>
    {
        private readonly IHubContext<MeetingHub> _meetingHub;
        private readonly IUserSessionDataProvider _userSessionDataProvider;
        private readonly IMeetingSessionDataProvider _meetingSessionDataProvider;

        public UserSessionWebRtcConnectionRemovedEventHandler(IHubContext<MeetingHub> meetingHub, 
            IUserSessionDataProvider userSessionDataProvider, IMeetingSessionDataProvider meetingSessionDataProvider)
        {
            _meetingHub = meetingHub;
            _userSessionDataProvider = userSessionDataProvider;
            _meetingSessionDataProvider = meetingSessionDataProvider;
        }

        public async Task Handle(IReceiveContext<UserSessionWebRtcConnectionRemovedEvent> context, CancellationToken cancellationToken)
        {
            var userSession = await _userSessionDataProvider
                .GetUserSessionById(context.Message.RemovedConnection.UserSessionId, cancellationToken).ConfigureAwait(false);
            
            var meetingSession = await _meetingSessionDataProvider
                .GetMeetingSessionById(userSession.MeetingSessionId, cancellationToken).ConfigureAwait(false);
            
            await _meetingHub.Clients
                .GroupExcept(meetingSession.MeetingNumber, userSession.ConnectionId)
                .SendAsync("OtherUserSessionWebRtcConnectionRemoved", context.Message.RemovedConnection, cancellationToken).ConfigureAwait(false);
        }
    }
}