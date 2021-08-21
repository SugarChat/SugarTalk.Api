using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kurento.NET;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SugarTalk.Core.Entities;
using SugarTalk.Core.Hubs.Exceptions;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Hubs
{
    [Authorize]
    public class MeetingHub : DynamicHub
    {
        private string UserName => Context.GetHttpContext().Request.Query["userName"];
        private string MeetingNumber => Context.GetHttpContext().Request.Query["meetingNumber"];

        private readonly IMapper _mapper;
        private readonly KurentoClient _kurento;
        private readonly IUserService _userService;
        private readonly IUserSessionService _userSessionService;
        private readonly IMeetingSessionService _meetingSessionService;
        private readonly IUserSessionDataProvider _userSessionDataProvider;
        private readonly IMeetingSessionDataProvider _meetingSessionDataProvider;
        
        public MeetingHub(IMapper mapper, KurentoClient kurento, IUserService userService,
            IUserSessionService userSessionService, IMeetingSessionService meetingSessionService, 
            IUserSessionDataProvider userSessionDataProvider, IMeetingSessionDataProvider meetingSessionDataProvider)
        {
            _mapper = mapper;
            _kurento = kurento;
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

        public async Task ProcessCandidateAsync(string peerConnectionId, UserSessionWebRtcConnectionIceCandidate candidate)
        {
            if (candidate != null)
            {
                candidate.Id = Guid.NewGuid();
                candidate.WebRtcPeerConnectionId = peerConnectionId;

                await _userSessionService.AddUserSessionWebRtcConnectionIceCandidate(candidate).ConfigureAwait(false);
            }
        }
        
        public async Task ProcessOfferAsync(Guid userSessionId, string peerConnectionId, string offerSdp, 
            UserSessionWebRtcConnectionMediaType mediaType, Guid? receiveWebRtcConnectionId)
        {
            var meetingSession = await _meetingSessionDataProvider.GetMeetingSession(MeetingNumber)
                .ConfigureAwait(false);

            var connectionWithIceCandidates =
                await GetOrCreateWebRtcConnection(meetingSession, userSessionId, peerConnectionId, offerSdp, mediaType, receiveWebRtcConnectionId)
                    .ConfigureAwait(false);

            if (connectionWithIceCandidates.Connection == null) return;

            Clients.Caller.ProcessAnswer(connectionWithIceCandidates.Connection.Id, peerConnectionId,
                connectionWithIceCandidates.Connection.WebRtcPeerConnectionOfferSdp);
            
            connectionWithIceCandidates.IceCandidates.ForEach(candidate =>
            {
                Clients.Caller.AddCandidate(peerConnectionId, candidate);
            });
        }

        private async
            Task<(UserSessionWebRtcConnection Connection, List<UserSessionWebRtcConnectionIceCandidate> IceCandidates)>
            GetOrCreateWebRtcConnection(
                MeetingSessionDto meetingSession, Guid userSessionId, string peerConnectionId, string peerConnectionOfferSdp,
                UserSessionWebRtcConnectionMediaType mediaType, Guid? receiveWebRtcConnectionId)
        {
            var selfUserSession = meetingSession.UserSessions.SingleOrDefault(x => x.ConnectionId == Context.ConnectionId);

            // maybe self user leave the meeting
            if (selfUserSession == null) return (null, null);
            
            if (selfUserSession.Id == userSessionId)
            {
                var webRtcConnection = new UserSessionWebRtcConnection
                {
                    Id = Guid.NewGuid(),
                    MediaType = mediaType,
                    UserSessionId = selfUserSession.Id,
                    WebRtcPeerConnectionId = peerConnectionId,
                    WebRtcPeerConnectionOfferSdp = peerConnectionOfferSdp,
                    ConnectionStatus = UserSessionWebRtcConnectionStatus.Connected
                };

                await _userSessionService
                    .AddUserSessionWebRtcConnection(webRtcConnection).ConfigureAwait(false);
                
                return (webRtcConnection, new List<UserSessionWebRtcConnectionIceCandidate>());
            }
            
            var otherUserSession = meetingSession.UserSessions.SingleOrDefault(x => x.Id == userSessionId);

            var otherUserSessionSendConnection =
                otherUserSession?.WebRtcConnections
                    .Select(x => 
                        _mapper.Map<UserSessionWebRtcConnection>(x))
                    .SingleOrDefault(x =>
                        x.Id == receiveWebRtcConnectionId &&
                        x.ConnectionStatus == UserSessionWebRtcConnectionStatus.Connected);

            if (otherUserSessionSendConnection == null) return (null, null);
            
            var otherUserSessionSendConnectionCandidates = await _userSessionDataProvider
                .GetUserSessionWebRtcConnectionIceCandidatesByPeerConnectionId(otherUserSessionSendConnection
                    .WebRtcPeerConnectionId).ConfigureAwait(false);
            
            return (otherUserSessionSendConnection, otherUserSessionSendConnectionCandidates);
        }
    }
}