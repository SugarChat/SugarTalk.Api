using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Core.Hubs
{
    [Authorize]
    public class MeetingHub : DynamicHub
    {
        private string MeetingNumber => Context.GetHttpContext().Request.Query["meetingNumber"];

        private readonly IUserService _userService;
        private readonly IUserSessionService _userSessionService;
        private readonly IMeetingSessionService _meetingSessionService;
        private readonly IUserSessionDataProvider _userSessionDataProvider;
        private readonly IMeetingSessionDataProvider _meetingSessionDataProvider;
        
        public MeetingHub(IUserService userService,
            IUserSessionService userSessionService, IMeetingSessionService meetingSessionService, 
            IUserSessionDataProvider userSessionDataProvider, IMeetingSessionDataProvider meetingSessionDataProvider)
        {
            _userService = userService;
            _userSessionService = userSessionService;
            _meetingSessionService = meetingSessionService;
            _userSessionDataProvider = userSessionDataProvider;
            _meetingSessionDataProvider = meetingSessionDataProvider;
        }
        
        public override async Task OnConnectedAsync()
        {
            var user = await _userService.GetCurrentLoggedInUser().ConfigureAwait(false);
            
            var meetingSession = await _meetingSessionDataProvider.GetMeetingSession(MeetingNumber)
                .ConfigureAwait(false);

            await _meetingSessionService.ConnectUserToMeetingSession(user, meetingSession, Context.ConnectionId)
                .ConfigureAwait(false);
            
            var userSession = meetingSession.UserSessions.SingleOrDefault(x => x.ConnectionId == Context.ConnectionId);
            var otherUserSessions = meetingSession.UserSessions.Where(x => x.ConnectionId != Context.ConnectionId).ToList();
            
            await Groups.AddToGroupAsync(Context.ConnectionId, MeetingNumber).ConfigureAwait(false);
            
            Clients.Caller.SetLocalUser(userSession);
            Clients.Caller.SetOtherUsers(otherUserSessions);
            Clients.OthersInGroup(MeetingNumber).OtherJoined(userSession);
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userSession = await _userSessionDataProvider.GetUserSessionByConnectionId(Context.ConnectionId)
                .ConfigureAwait(false);
            
            Clients.OthersInGroup(MeetingNumber).OtherLeft(userSession);
            
            await _userSessionService.RemoveUserSession(userSession).ConfigureAwait(false);

            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }
        
        public void ProcessOffer(UserSessionDto sendFromUserSession, UserSessionDto sendToUserSession, 
            OfferPeerConnectionMediaType offerPeerConnectionMediaType, string offerPeerConnectionId, string offerToJson)
        {
            Clients.Client(sendToUserSession.ConnectionId)
                .OtherOfferSent(sendFromUserSession, offerPeerConnectionMediaType, offerPeerConnectionId, offerToJson);
        }
        
        public void ProcessAnswer(UserSessionDto sendFromUserSession, UserSessionDto sendToUserSession, string offerPeerConnectionId, string answerPeerConnectionId, string answerToJson)
        {
            Clients.Client(sendToUserSession.ConnectionId)
                .OtherAnswerSent(sendFromUserSession, offerPeerConnectionId, answerPeerConnectionId, answerToJson);
        }
        
        public void ProcessCandidate(UserSessionDto sendToUserSession, string peerConnectionId, string candidateToJson)
        {
            Clients.Client(sendToUserSession.ConnectionId)
                .OtherCandidateCreated(peerConnectionId, candidateToJson);
        }

        public void ConnectionsClosed(IEnumerable<string> peerConnectionIds)
        {
            Clients.OthersInGroup(MeetingNumber)
                .OtherConnectionsClosed(peerConnectionIds);
        }
        
        public void ConnectionNotFoundWhenOtherIceSent(UserSessionDto sendToUserSession, string peerConnectionId, string candidateToJson)
        {
            Clients.Client(sendToUserSession.ConnectionId)
                .OtherCandidateCreated(peerConnectionId, candidateToJson);
        }
    }

    public enum OfferPeerConnectionMediaType
    {
        Audio,
        Video,
        Screen
    }
}