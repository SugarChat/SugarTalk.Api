using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kurento.NET;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Core.Hubs
{
    [Authorize]
    public class P2pMeetingHub : DynamicHub
    {
        private string MeetingNumber => Context.GetHttpContext().Request.Query["meetingNumber"];

        private readonly IMapper _mapper;
        private readonly KurentoClient _kurento;
        private readonly IUserService _userService;
        private readonly IUserSessionService _userSessionService;
        private readonly IMeetingSessionService _meetingSessionService;
        private readonly IUserSessionDataProvider _userSessionDataProvider;
        private readonly IMeetingSessionDataProvider _meetingSessionDataProvider;
        
        public P2pMeetingHub(IMapper mapper, KurentoClient kurento, IUserService userService,
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

        public void ProcessCandidate(UserSessionDto sendFromUserSession, UserSessionDto sendToUserSession, string peerConnectionId, string candidateToJson)
        {
            Clients.Client(sendToUserSession.ConnectionId)
                .OtherCandidateCreated(sendFromUserSession, peerConnectionId, candidateToJson);
        }
        
        public void ProcessOffer(UserSessionDto sendFromUserSession, UserSessionDto sendToUserSession, string offerPeerConnectionId, string offerToJson)
        {
            Clients.Client(sendToUserSession.ConnectionId)
                .OtherOfferSent(sendFromUserSession, offerPeerConnectionId, offerToJson);
        }
        
        public void ProcessAnswer(UserSessionDto sendFromUserSession, UserSessionDto sendToUserSession, string offerPeerConnectionId, string answerPeerConnectionId, string answerToJson)
        {
            Clients.Client(sendToUserSession.ConnectionId)
                .OtherAnswerSent(sendFromUserSession, offerPeerConnectionId, answerPeerConnectionId, answerToJson);
        }
    }
}