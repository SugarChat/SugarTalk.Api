using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Kurento.NET;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace SugarTalk.Core.Services.Kurento
{
    public class MeetingHub : DynamicHub
    {
        private readonly KurentoClient _kurento;
        private readonly MeetingSessionManager _meetingSessionManager;

        public MeetingHub(KurentoClient kurento, MeetingSessionManager meetingSessionManager)
        {
            _kurento = kurento;
            _meetingSessionManager = meetingSessionManager;
        }
        
        public string UserName => Context.GetHttpContext().Request.Query["userName"];

        public Guid MeetingId => Guid.Parse(Context.GetHttpContext().Request.Query["meetingId"]);

        public override async Task OnConnectedAsync()
        {
            var meetingSession = await _meetingSessionManager.GetMeetingSessionAsync(MeetingId).ConfigureAwait(false);
            
            var userSession = new UserSession
            {
                Id = Context.ConnectionId,
                UserName = UserName,
                SendEndPoint = null,
                ReceviedEndPoints = new ConcurrentDictionary<string, WebRtcEndpoint>()
            };
            
            meetingSession.UserSessions.TryAdd(Context.ConnectionId, userSession);
            await Groups.AddToGroupAsync(Context.ConnectionId, MeetingId.ToString()).ConfigureAwait(false);
            Clients.Caller.SetLocalUser(userSession);
            Clients.Caller.SetOtherUsers(meetingSession.GetOtherUsers(Context.ConnectionId));
            Clients.OthersInGroup(MeetingId.ToString()).OtherJoined(userSession);
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var meetingSession = await _meetingSessionManager.GetMeetingSessionAsync(MeetingId).ConfigureAwait(false);
            await meetingSession.RemoveAsync(Context.ConnectionId).ConfigureAwait(false);
            Clients.OthersInGroup(MeetingId.ToString()).OtherLeft(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }
        
        private async Task<WebRtcEndpoint> GetEndPointAsync(string id)
        {
            var meetingSession = await _meetingSessionManager.GetMeetingSessionAsync(MeetingId).ConfigureAwait(false);
            
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
            var answerSDP = await endPonit.ProcessOfferAsync(offerSdp).ConfigureAwait(false);
            Clients.Caller.ProcessAnswer(id, answerSDP);
            await endPonit.GatherCandidatesAsync().ConfigureAwait(false);
        }
    }
}