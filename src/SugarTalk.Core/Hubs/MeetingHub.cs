using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kurento.NET;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SugarTalk.Core.Entities;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages.Dtos.Meetings;

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
            
            await _userSessionService.RemoveUserSession(userSession).ConfigureAwait(false);
            
            Clients.OthersInGroup(MeetingNumber).OtherLeft(userSession);
            
            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }

        public async Task ProcessCandidateAsync(string connectionId, string peerConnectionId, IceCandidate candidate)
        {
            var meetingSession = await _meetingSessionDataProvider.GetMeetingSession(MeetingNumber)
                .ConfigureAwait(false);
            
            var endPoint = await CreateOrUpdateEndpointAsync(connectionId, peerConnectionId, meetingSession, false)
                .ConfigureAwait(false);

            await endPoint.AddIceCandidateAsync(candidate);
        }
        
        public async Task ProcessOfferAsync(string connectionId, string offerSdp, bool isSharingCamera, bool isSharingScreen, string peerConnectionId)
        {
            var meetingSession = await _meetingSessionDataProvider.GetMeetingSession(MeetingNumber)
                .ConfigureAwait(false);

            var userSession = meetingSession.UserSessions.SingleOrDefault(x => x.ConnectionId == connectionId);

            if (userSession != null)
            {
                userSession.IsSharingCamera = isSharingCamera;
                userSession.IsSharingScreen = isSharingScreen;

                await _userSessionService.UpdateUserSession(_mapper.Map<UserSession>(userSession))
                    .ConfigureAwait(false);
            }

            var endPoint = await CreateOrUpdateEndpointAsync(connectionId, peerConnectionId, meetingSession, true)
                .ConfigureAwait(false);

            var answerSdp = await endPoint.ProcessOfferAsync(offerSdp).ConfigureAwait(false);

            Clients.Caller.ProcessAnswer(connectionId, answerSdp, isSharingCamera, isSharingScreen, peerConnectionId);
            
            await endPoint.GatherCandidatesAsync().ConfigureAwait(false);
        }
        
        private async Task<WebRtcEndpoint> CreateOrUpdateEndpointAsync(string connectionId,
            string peerConnectionId, MeetingSessionDto meetingSession, bool shouldRecreateSendEndPoint)
        {
            var selfSession = meetingSession.UserSessions.SingleOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (selfSession == null) return default;

            if (selfSession.ConnectionId == connectionId)
            {
                if (selfSession.SendEndPoint == null ||
                    selfSession.SendEndPoint != null && shouldRecreateSendEndPoint)
                {
                    selfSession.SendEndPoint =
                        await CreateEndPoint(connectionId, peerConnectionId, meetingSession.Pipeline).ConfigureAwait(false);
                    await _userSessionService
                        .UpdateUserSessionEndpoints(selfSession.Id, selfSession.SendEndPoint, selfSession.ReceivedEndPoints).ConfigureAwait(false);
                }

                return selfSession.SendEndPoint;
            }

            var otherSession = meetingSession.UserSessions.SingleOrDefault(x => x.ConnectionId == connectionId);
            
            if (otherSession == null) return default;
            
            otherSession.SendEndPoint ??= await CreateEndPoint(connectionId, peerConnectionId, meetingSession.Pipeline).ConfigureAwait(false);

            selfSession.ReceivedEndPoints.TryGetValue(connectionId, out var otherEndPoint);

            if (otherEndPoint == null || shouldRecreateSendEndPoint)
            {
                otherEndPoint = await CreateEndPoint(connectionId, peerConnectionId, meetingSession.Pipeline).ConfigureAwait(false);
                selfSession.ReceivedEndPoints.TryAdd(connectionId, otherEndPoint);
                await otherSession.SendEndPoint.ConnectAsync(otherEndPoint).ConfigureAwait(false);
                await _userSessionService
                    .UpdateUserSessionEndpoints(selfSession.Id, selfSession.SendEndPoint, selfSession.ReceivedEndPoints).ConfigureAwait(false);
            }
            
            await _userSessionService
                .UpdateUserSessionEndpoints(otherSession.Id, otherSession.SendEndPoint, otherSession.ReceivedEndPoints).ConfigureAwait(false);
            
            return otherEndPoint;
        }
        
        private async Task<WebRtcEndpoint> CreateEndPoint(string connectionId, string peerConnectionId, MediaPipeline pipeline)
        {
            var endPoint = await _kurento.CreateAsync(new WebRtcEndpoint(pipeline)).ConfigureAwait(false);
            
            endPoint.OnIceCandidate += arg =>
            {
                Clients.Caller.AddCandidate(connectionId, JsonConvert.SerializeObject(arg.candidate), peerConnectionId);
            };

            await endPoint.SetMinVideoSendBandwidthAsync(8192);
            await endPoint.SetMinVideoRecvBandwidthAsync(8192);
            
            return endPoint;
        }
    }
}