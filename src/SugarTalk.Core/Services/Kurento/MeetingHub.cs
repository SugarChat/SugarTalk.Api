using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Kurento.NET;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SugarTalk.Messages;
using SugarTalk.Messages.Dtos;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Requests;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Kurento
{
    [Authorize]
    public class MeetingHub : DynamicHub
    {
        private string UserName => Context.GetHttpContext().Request.Query["userName"];
        private string MeetingNumber => Context.GetHttpContext().Request.Query["meetingNumber"];

        private readonly IMediator _mediator;
        private readonly KurentoClient _kurento;
        private readonly MeetingSessionManager _meetingSessionManager;

        public MeetingHub(IMediator mediator, KurentoClient kurento, MeetingSessionManager meetingSessionManager)
        {
            _kurento = kurento;
            _mediator = mediator;
            _meetingSessionManager = meetingSessionManager;
        }

        public override async Task OnConnectedAsync()
        {
            var meeting = await GetMeeting().ConfigureAwait(false);
            var meetingSession = await _meetingSessionManager.GetOrCreateMeetingSessionAsync(meeting)
                .ConfigureAwait(false);
            var userName = string.IsNullOrEmpty(UserName) ? Context.User.Identity.Name : UserName;
            var userSession = new UserSession
            {
                Id = Context.ConnectionId,
                UserName = userName,
                SendEndPoint = null,
                ReceviedEndPoints = new ConcurrentDictionary<string, WebRtcEndpoint>()
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
        
        private async Task<WebRtcEndpoint> GetEndPointAsync(string id)
        {
            var meeting = await GetMeeting().ConfigureAwait(false);
            var meetingSession = await _meetingSessionManager.GetOrCreateMeetingSessionAsync(meeting)
                .ConfigureAwait(false);
            
            if (meetingSession.UserSessions.TryGetValue(Context.ConnectionId, out var selfSession))
            {
                if (Context.ConnectionId == id)
                {
                    if (selfSession.SendEndPoint == null)
                    {
                        var endPoint = new WebRtcEndpoint(meetingSession.Pipeline);
                        
                        selfSession.SendEndPoint = await _kurento.CreateAsync(endPoint).ConfigureAwait(false);
                        selfSession.SendEndPoint.OnIceCandidate += arg =>
                        {
                            Clients.Caller.AddCandidate(id, JsonConvert.SerializeObject(arg.candidate));
                        };
                    }
                    return selfSession.SendEndPoint;
                }
                else
                {
                    if (meetingSession.UserSessions.TryGetValue(id, out var otherSession))
                    {
                        if (otherSession.SendEndPoint == null)
                        {
                            otherSession.SendEndPoint = await _kurento.CreateAsync(new WebRtcEndpoint(meetingSession.Pipeline)).ConfigureAwait(false);
                            otherSession.SendEndPoint.OnIceCandidate += arg =>
                            {
                                Clients.Client(id).AddCandidate(id, JsonConvert.SerializeObject(arg.candidate));
                            };
                        }
                        if (!selfSession.ReceviedEndPoints.TryGetValue(id, out WebRtcEndpoint otherEndPoint))
                        {
                            otherEndPoint = await _kurento.CreateAsync(new WebRtcEndpoint(meetingSession.Pipeline)).ConfigureAwait(false);
                            otherEndPoint.OnIceCandidate += arg =>
                            {
                                Clients.Caller.AddCandidate(id, JsonConvert.SerializeObject(arg.candidate));
                            };
                            await otherSession.SendEndPoint.ConnectAsync(otherEndPoint).ConfigureAwait(false);
                            selfSession.ReceviedEndPoints.TryAdd(id, otherEndPoint);
                        }
                        return otherEndPoint;
                    }
                }
            }
            return default(WebRtcEndpoint);
        }
        
        public async Task ProcessCandidateAsync(string id, IceCandidate candidate)
        {
            var endPonit = await GetEndPointAsync(id).ConfigureAwait(false);
            await endPonit.AddIceCandidateAsync(candidate);
        }
        
        public async Task ProcessOfferAsync(string id, string offerSdp)
        {
            var endPonit = await GetEndPointAsync(id).ConfigureAwait(false);
            var answerSdp = await endPonit.ProcessOfferAsync(offerSdp).ConfigureAwait(false);
            Clients.Caller.ProcessAnswer(id, answerSdp);
            await endPonit.GatherCandidatesAsync().ConfigureAwait(false);
        }
    }
}