using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Hubs;

[Authorize]
public class MeetingHub : DynamicHub
{
    private string MeetingNumber => Context.GetHttpContext().Request.Query["meetingNumber"];

    private readonly ICurrentUser _currentUser;
    private readonly IMeetingService _meetingService;
    private readonly IAccountDataProvider _accountDataProvider;
    private readonly IMeetingDataProvider _meetingDataProvider;

    public MeetingHub(ICurrentUser currentUser, IMeetingService meetingService,
        IAccountDataProvider accountDataProvider, IMeetingDataProvider meetingDataProvider)
    {
        _currentUser = currentUser;
        _meetingService = meetingService;
        _accountDataProvider = accountDataProvider;
        _meetingDataProvider = meetingDataProvider;
    }

    public override async Task OnConnectedAsync()
    {
        var user = await _accountDataProvider.GetUserAccountAsync(id: _currentUser.Id.Value).ConfigureAwait(false);

        var meetingSession = await _meetingDataProvider.GetMeetingAsync(MeetingNumber).ConfigureAwait(false);

        await _meetingService.ConnectUserToMeetingAsync(user, meetingSession, Context.ConnectionId, MeetingStreamType.Audio).ConfigureAwait(false);

        var userSession = meetingSession.UserSessions.SingleOrDefault(x =>
            x.UserSessionStreams.FirstOrDefault()?.StreamId == Context.ConnectionId);
        var otherUserSessions = meetingSession.UserSessions
            .Where(x => x.UserSessionStreams.FirstOrDefault()?.StreamId != Context.ConnectionId).ToList();

        await Groups.AddToGroupAsync(Context.ConnectionId, MeetingNumber).ConfigureAwait(false);

        Clients.Caller.SetLocalUser(userSession);
        Clients.Caller.SetOtherUsers(otherUserSessions);
        Clients.OthersInGroup(MeetingNumber).OtherJoined(userSession);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // var userSession = await _meetingDataProvider.GetUserSessionsByMeetingIdAsync()

        var userSession = new MeetingUserSession();        
        
        if (userSession != null)
        {
            Clients.OthersInGroup(MeetingNumber).OtherLeft(userSession);

            await _meetingDataProvider.RemoveMeetingUserSessionsAsync(new List<MeetingUserSession>{userSession}).ConfigureAwait(false);
        }

        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }

    public void ProcessOffer(MeetingUserSessionDto sendFromUserSession, MeetingUserSessionDto sendToUserSession,
        OfferPeerConnectionMediaType offerPeerConnectionMediaType, string offerPeerConnectionId, string offerToJson,
        string[] candidatesToJson)
    {
        Clients.Client(sendToUserSession.UserSessionStreams?.FirstOrDefault()?.StreamId)
            .OtherOfferSent(sendFromUserSession, offerPeerConnectionMediaType, offerPeerConnectionId, offerToJson,
                candidatesToJson);
    }

    public void ProcessAnswer(MeetingUserSessionDto sendFromUserSession, MeetingUserSessionDto sendToUserSession,
        string offerPeerConnectionId, string answerPeerConnectionId, string answerToJson, string[] candidatesToJson)
    {
        Clients.Client(sendToUserSession.UserSessionStreams?.FirstOrDefault()?.StreamId)
            .OtherAnswerSent(sendFromUserSession, offerPeerConnectionId, answerPeerConnectionId, answerToJson,
                candidatesToJson);
    }

    public void ConnectionsClosed(IEnumerable<string> peerConnectionIds)
    {
        Clients.OthersInGroup(MeetingNumber)
            .OtherConnectionsClosed(peerConnectionIds);
    }
    
    public enum OfferPeerConnectionMediaType
    {
        Audio,
        Video,
        Screen
    }
}
