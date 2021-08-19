using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kurento.NET;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Serilog;
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

        public async Task ProcessCandidateAsync(Guid userSessionId, string peerConnectionId, IceCandidate candidate, 
            UserSessionWebRtcConnectionMediaType mediaType, Guid? receiveWebRtcConnectionId)
        {
            var meetingSession = await _meetingSessionDataProvider.GetMeetingSession(MeetingNumber)
                .ConfigureAwait(false);
            
            var connection =
                await GetOrCreateWebRtcConnection(meetingSession, userSessionId, peerConnectionId, mediaType, receiveWebRtcConnectionId)
                    .ConfigureAwait(false);

            await connection.WebRtcEndpoint.AddIceCandidateAsync(candidate);
        }
        
        public async Task ProcessOfferAsync(Guid userSessionId, string peerConnectionId, string offerSdp, 
            UserSessionWebRtcConnectionMediaType mediaType, Guid? receiveWebRtcConnectionId)
        {
            var meetingSession = await _meetingSessionDataProvider.GetMeetingSession(MeetingNumber)
                .ConfigureAwait(false);

            var connection =
                await GetOrCreateWebRtcConnection(meetingSession, userSessionId, peerConnectionId, mediaType, receiveWebRtcConnectionId)
                    .ConfigureAwait(false);

            var answerSdp = await connection.WebRtcEndpoint.ProcessOfferAsync(offerSdp).ConfigureAwait(false);

            Clients.Caller.ProcessAnswer(connection.Id, peerConnectionId, answerSdp);
            
            await connection.WebRtcEndpoint.GatherCandidatesAsync().ConfigureAwait(false);
        }
        
        private async Task<UserSessionWebRtcConnectionDto> GetOrCreateWebRtcConnection(
            MeetingSessionDto meetingSession, Guid userSessionId, string peerConnectionId,
            UserSessionWebRtcConnectionMediaType mediaType, Guid? receiveWebRtcConnectionId)
        {
            var selfUserSession = meetingSession.UserSessions.Single(x => x.ConnectionId == Context.ConnectionId);
            
            if (selfUserSession.Id == userSessionId)
            {
                var webRtcConnection =
                    selfUserSession.WebRtcConnections.SingleOrDefault(x =>
                        x.WebRtcPeerConnectionId == peerConnectionId);

                if (webRtcConnection != null)
                    return webRtcConnection;

                var selfEndpoint = await CreateEndPoint(meetingSession.Pipeline, peerConnectionId, mediaType)
                    .ConfigureAwait(false);

                webRtcConnection = new UserSessionWebRtcConnectionDto
                {
                    Id = Guid.NewGuid(),
                    MediaType = mediaType,
                    UserSessionId = selfUserSession.Id,
                    WebRtcEndpoint = selfEndpoint,
                    WebRtcEndpointId = selfEndpoint.id,
                    WebRtcPeerConnectionId = peerConnectionId,
                    ConnectionType = UserSessionWebRtcConnectionType.Send
                };

                await _userSessionService
                    .AddUserSessionWebRtcConnection(_mapper.Map<UserSessionWebRtcConnection>(webRtcConnection)).ConfigureAwait(false);
                
                return webRtcConnection;
            }
            
            var otherUserSession = meetingSession.UserSessions.Single(x => x.Id == userSessionId);

            var otherUserSessionSendEndpointConnection =
                otherUserSession.WebRtcConnections.SingleOrDefault(x =>
                    x.Id == receiveWebRtcConnectionId && 
                    x.ConnectionType == UserSessionWebRtcConnectionType.Send &&
                    x.ConnectionStatus == UserSessionWebRtcConnectionStatus.Connected);

            if (otherUserSessionSendEndpointConnection == null)
                throw new UserSessionWebRtcConnectionNotFoundException();
            
            var receiveThisUserSessionEndpointConnection = selfUserSession.WebRtcConnections
                .SingleOrDefault(x => x.ReceiveWebRtcConnectionId == otherUserSessionSendEndpointConnection.Id);

            if (receiveThisUserSessionEndpointConnection != null)
                return receiveThisUserSessionEndpointConnection;
            
            var receiveThisUserSessionEndpoint = await CreateEndPoint(meetingSession.Pipeline, peerConnectionId, mediaType).ConfigureAwait(false);

            await otherUserSessionSendEndpointConnection.WebRtcEndpoint.ConnectAsync(receiveThisUserSessionEndpoint)
                .ConfigureAwait(false);
            
            receiveThisUserSessionEndpointConnection = new UserSessionWebRtcConnectionDto
            {
                Id = Guid.NewGuid(),
                MediaType = mediaType,
                UserSessionId = selfUserSession.Id,
                WebRtcPeerConnectionId = peerConnectionId,
                WebRtcEndpoint = receiveThisUserSessionEndpoint,
                WebRtcEndpointId = receiveThisUserSessionEndpoint.id,
                ConnectionType = UserSessionWebRtcConnectionType.Receive,
                ReceiveWebRtcConnectionId = receiveWebRtcConnectionId
            };

            await _userSessionService
                .AddUserSessionWebRtcConnection(_mapper.Map<UserSessionWebRtcConnection>(receiveThisUserSessionEndpointConnection)).ConfigureAwait(false);

            return receiveThisUserSessionEndpointConnection;
        }
        
        private async Task<WebRtcEndpoint> CreateEndPoint(MediaPipeline pipeline, string peerConnectionId, UserSessionWebRtcConnectionMediaType mediaType)
        {
            var endPoint = await _kurento.CreateAsync(new WebRtcEndpoint(pipeline)).ConfigureAwait(false);

            if (mediaType == UserSessionWebRtcConnectionMediaType.Screen)
            {
                await endPoint.SetMinVideoSendBandwidthAsync(100000).ConfigureAwait(false);
                await endPoint.SetMinVideoRecvBandwidthAsync(100000).ConfigureAwait(false);

                Log.Information($"Share screen MinOutputBitrate is {await endPoint.GetMinOutputBitrateAsync()}");
                Log.Information($"Share screen MaxOutputBitrate is {await endPoint.GetMaxOutputBitrateAsync()}");
                
                await endPoint.SetMinOutputBitrateAsync(25000).ConfigureAwait(false);
            }
            
            endPoint.OnIceCandidate += arg =>
            {
                Clients.Caller.AddCandidate(peerConnectionId, JsonConvert.SerializeObject(arg.candidate));
            };
            
            return endPoint;
        }
    }
}