using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Kurento.NET;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Serilog;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Requests.Users;

namespace SugarTalk.Core.Services.Kurento
{
    [Authorize]
    public class MeetingHub : DynamicHub
    {
        private string UserName => Context.GetHttpContext().Request.Query["userName"];
        private string MeetingNumber => Context.GetHttpContext().Request.Query["meetingNumber"];

        private readonly IMediator _mediator;
        private readonly KurentoClient _kurento;
        private readonly IUserService _userService;
        private readonly MeetingSessionManager _meetingSessionManager;

        public MeetingHub(IMediator mediator, KurentoClient kurento, MeetingSessionManager meetingSessionManager, IUserService userService)
        {
            _kurento = kurento;
            _mediator = mediator;
            _userService = userService;
            _meetingSessionManager = meetingSessionManager;
        }

        public override async Task OnConnectedAsync()
        {
            var response = await _userService.SignInFromThirdParty(new SignInFromThirdPartyRequest(), default)
                .ConfigureAwait(false);
            var user = response.Data;
            var meeting = await GetMeeting().ConfigureAwait(false);
            var meetingSession = await _meetingSessionManager.GetOrCreateMeetingSessionAsync(meeting)
                .ConfigureAwait(false);
            var userName = string.IsNullOrEmpty(UserName) ? user.DisplayName : UserName;
            var userSession = new UserSession
            {
                Id = Context.ConnectionId,
                UserId = user.Id,
                UserName = userName,
                SendEndPoint = null,
                ReceivedEndPoints = new ConcurrentDictionary<string, WebRtcEndpoint>()
            };
            meetingSession.UserSessions.TryAdd(Context.ConnectionId, userSession);
            await Groups.AddToGroupAsync(Context.ConnectionId, MeetingNumber).ConfigureAwait(false);
            Clients.Caller.SetLocalUser(userSession);
            Clients.Caller.SetOtherUsers(meetingSession.GetOtherUsers(Context.ConnectionId));
            Clients.OthersInGroup(MeetingNumber).OtherJoined(userSession);
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var meeting = await GetMeeting().ConfigureAwait(false);
            var meetingSession = await _meetingSessionManager.GetOrCreateMeetingSessionAsync(meeting)
                .ConfigureAwait(false);
            await meetingSession.RemoveAsync(Context.ConnectionId).ConfigureAwait(false);
            Clients.OthersInGroup(MeetingNumber).OtherLeft(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }

        private async Task<MeetingDto> GetMeeting()
        {
            var meeting = await _mediator.RequestAsync<GetMeetingByNumberRequest, SugarTalkResponse<MeetingDto>>(
                new GetMeetingByNumberRequest
                {
                    MeetingNumber = MeetingNumber
                }).ConfigureAwait(false);

            return meeting.Data;
        }
        
        private async Task<WebRtcEndpoint> GetEndPointAsync(string connectionId, bool shouldRecreateSendEndPoint)
        {
            var meeting = await GetMeeting().ConfigureAwait(false);
            var meetingSession = await _meetingSessionManager.GetOrCreateMeetingSessionAsync(meeting)
                .ConfigureAwait(false);
            
            if (meetingSession.UserSessions.TryGetValue(Context.ConnectionId, out var selfSession))
            {
                if (Context.ConnectionId == connectionId)
                {
                    if (selfSession.SendEndPoint == null)
                    {
                        await CreateEndPoint(connectionId, selfSession, meetingSession);
                    }
                    else
                    {
                        if (shouldRecreateSendEndPoint)
                            await CreateEndPoint(connectionId, selfSession, meetingSession);
                    }
                    return selfSession.SendEndPoint;
                }
                else
                {
                    if (meetingSession.UserSessions.TryGetValue(connectionId, out var otherSession))
                    {
                        if (otherSession.SendEndPoint == null)
                        {
                            await CreateEndPoint(connectionId, otherSession, meetingSession);
                        }
                        if (!selfSession.ReceivedEndPoints.TryGetValue(connectionId, out WebRtcEndpoint otherEndPoint))
                        {
                            otherEndPoint = await _kurento.CreateAsync(new WebRtcEndpoint(meetingSession.Pipeline)).ConfigureAwait(false);
                            otherEndPoint.OnIceCandidate += arg =>
                            {
                                Clients.Caller.AddCandidate(connectionId, JsonConvert.SerializeObject(arg.candidate));
                            };
                            await otherSession.SendEndPoint.ConnectAsync(otherEndPoint).ConfigureAwait(false);
                            selfSession.ReceivedEndPoints.TryAdd(connectionId, otherEndPoint);
                        }
                        else
                        {
                            if (shouldRecreateSendEndPoint)
                            {
                                otherEndPoint = await _kurento.CreateAsync(new WebRtcEndpoint(meetingSession.Pipeline)).ConfigureAwait(false);
                                otherEndPoint.OnIceCandidate += arg =>
                                {
                                    Clients.Caller.AddCandidate(connectionId, JsonConvert.SerializeObject(arg.candidate));
                                };
                                await otherSession.SendEndPoint.ConnectAsync(otherEndPoint).ConfigureAwait(false);
                                selfSession.ReceivedEndPoints.TryAdd(connectionId, otherEndPoint);
                            }
                        }
                        return otherEndPoint;
                    }
                }
            }
            return default(WebRtcEndpoint);
        }
        
        public async Task ProcessCandidateAsync(string connectionId, IceCandidate candidate)
        {
            var endPonit = await GetEndPointAsync(connectionId, false).ConfigureAwait(false);
            await endPonit.AddIceCandidateAsync(candidate);
        }
        
        public async Task ProcessOfferAsync(string connectionId, string offerSdp, bool isNew, bool isSharingCamera, bool isSharingScreen)
        {
            var endPonit = await GetEndPointAsync(connectionId, true).ConfigureAwait(false);

            var answerSdp = await endPonit.ProcessOfferAsync(offerSdp).ConfigureAwait(false);
            Clients.Caller.ProcessAnswer(connectionId, answerSdp, isSharingCamera, isSharingScreen);
            if (isNew)
                Clients.OthersInGroup(MeetingNumber).NewOfferCreated(connectionId, offerSdp);
            await endPonit.GatherCandidatesAsync().ConfigureAwait(false);
        }

        private async Task<WebRtcEndpoint> CreateEndPoint(string connectionId, UserSession selfSession, MeetingSession meetingSession)
        {
            var endPoint = new WebRtcEndpoint(meetingSession.Pipeline);
            
            selfSession.SendEndPoint = await _kurento.CreateAsync(endPoint).ConfigureAwait(false);
            selfSession.SendEndPoint.OnIceCandidate += arg =>
            {
                Clients.Caller.AddCandidate(connectionId, JsonConvert.SerializeObject(arg.candidate));
            };
            return endPoint;
        }
    }
}