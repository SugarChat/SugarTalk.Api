using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Core.Services.AntMediaServer;


namespace SugarTalk.Core.Hubs;

[Authorize]
public class MeetingHub : DynamicHub
{
    private const string appName = "LiveApp";
    private string streamId => Context.ConnectionId;
    private string meetingNumber => Context.GetHttpContext()?.Request.Query["meetingNumber"];

    private readonly ICurrentUser _currentUser;
    private readonly IMeetingService _meetingService;
    private readonly IAccountDataProvider _accountDataProvider;
    private readonly IMeetingDataProvider _meetingDataProvider;
    private readonly IAntMediaServerUtilService _antMediaServerUtilService;

    public MeetingHub(
        ICurrentUser currentUser, 
        IMeetingService meetingService,
        IAccountDataProvider accountDataProvider, 
        IMeetingDataProvider meetingDataProvider, 
        IAntMediaServerUtilService antMediaServerUtilService)
    {
        _currentUser = currentUser;
        _meetingService = meetingService;
        _accountDataProvider = accountDataProvider;
        _meetingDataProvider = meetingDataProvider;
        _antMediaServerUtilService = antMediaServerUtilService;
    }

    public override async Task OnConnectedAsync()
    {
        var user = await _accountDataProvider.GetUserAccountAsync(id: _currentUser.Id.Value).ConfigureAwait(false);

        var meetingSession = await _meetingDataProvider.GetMeetingAsync(meetingNumber).ConfigureAwait(false);

        await _meetingService.ConnectUserToMeetingAsync(user, meetingSession, streamId, MeetingStreamType.Audio).ConfigureAwait(false);

        var userSession = meetingSession.UserSessions.SingleOrDefault(
            x => x.UserSessionStreams.FirstOrDefault()?.StreamId == streamId);
        
        var otherUserSessions = meetingSession.UserSessions
            .Where(x => x.UserSessionStreams.FirstOrDefault()?.StreamId != streamId).ToList();

        await Groups.AddToGroupAsync(streamId, meetingNumber).ConfigureAwait(false);

        Clients.Caller.SetLocalUser(userSession);
        Clients.Caller.SetOtherUsers(otherUserSessions);
        Clients.OthersInGroup(meetingNumber).OtherJoined(userSession);
    }

    public async Task<MeetingDto> GetMeetingInfoAsync(bool includeUserSession = true)
    {
        var meetingResponse = await _meetingService
            .GetMeetingByNumberAsync(new GetMeetingByNumberRequest { MeetingNumber = meetingNumber, IncludeUserSession = includeUserSession}).ConfigureAwait(false);
        
        return meetingResponse.Data;
    }
    
    public async Task DrawOnCanvasAsync(string drawingData)
    {
        var userSession = await _meetingDataProvider.GetUserSessionByStreamIdAsync(streamId).ConfigureAwait(false);
        
        if (userSession != null)
            Clients.OthersInGroup(meetingNumber).CanvasDrawingReceived(userSession.UserId, drawingData);
    }
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userSession = await _meetingDataProvider.GetUserSessionByStreamIdAsync(streamId).ConfigureAwait(false);
        
        if (userSession != null)
        {
            await _antMediaServerUtilService
                .RemoveStreamFromMeetingAsync(appName, meetingNumber, streamId).ConfigureAwait(false);

            await _meetingDataProvider
                .RemoveMeetingUserSessionStreamsAsync(new List<int> { userSession.Id }).ConfigureAwait(false);
            
            await _meetingDataProvider
                .RemoveMeetingUserSessionsAsync(new List<MeetingUserSession> { userSession }).ConfigureAwait(false);
            
            Clients.OthersInGroup(meetingNumber).OtherLeft(userSession);
        }
        
        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }

    public async Task ConnectionsClosedAsync(IEnumerable<string> peerConnectionIds)
    {
        await _meetingService.EndMeetingAsync(new EndMeetingCommand { MeetingNumber = meetingNumber }).ConfigureAwait(false);
        
        Clients.OthersInGroup(meetingNumber).OtherConnectionsClosed(peerConnectionIds);
    }
    
    public void ProcessOffer(MeetingUserSessionDto sendFromUserSession, MeetingUserSessionDto sendToUserSession,
        OfferPeerConnectionMediaType offerPeerConnectionMediaType, string offerPeerConnectionId, string offerToJson, string[] candidatesToJson)
    {
        Clients.Client(sendToUserSession.UserSessionStreams?.FirstOrDefault()?.StreamId)
            .OtherOfferSent(sendFromUserSession, offerPeerConnectionMediaType, offerPeerConnectionId, offerToJson, candidatesToJson);
    }

    public void ProcessAnswer(MeetingUserSessionDto sendFromUserSession, MeetingUserSessionDto sendToUserSession,
        string offerPeerConnectionId, string answerPeerConnectionId, string answerToJson, string[] candidatesToJson)
    {
        Clients.Client(sendToUserSession.UserSessionStreams?.FirstOrDefault()?.StreamId)
            .OtherAnswerSent(sendFromUserSession, offerPeerConnectionId, answerPeerConnectionId, answerToJson, candidatesToJson);
    }
    
    public enum OfferPeerConnectionMediaType
    {
        Audio,
        Video
    }
}
