using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Hubs;

[Authorize]
public class MeetingHub : DynamicHub
{
    private string streamId => Context.ConnectionId;
    private string meetingNumber => Context.GetHttpContext()?.Request.Query["meetingNumber"];

    private readonly IMeetingService _meetingService;
    private readonly IMeetingDataProvider _meetingDataProvider;

    public MeetingHub(
        IMeetingService meetingService,
        IMeetingDataProvider meetingDataProvider)
    {
        _meetingService = meetingService;
        _meetingDataProvider = meetingDataProvider;
    }

    public override async Task OnConnectedAsync()
    {
        var meetingSession = await _meetingDataProvider.GetMeetingAsync(meetingNumber).ConfigureAwait(false);

        var userSession = meetingSession.UserSessions.SingleOrDefault(
            x => x.UserSessionStreams.FirstOrDefault()?.StreamId == streamId);
        
        var otherUserSessions = meetingSession.UserSessions
            .Where(x => x.UserSessionStreams.FirstOrDefault()?.StreamId != streamId).ToList();

        await Groups.AddToGroupAsync(streamId, meetingNumber).ConfigureAwait(false);

        await Clients.Caller.SetLocalUser(userSession);
        await Clients.Caller.SetOtherUsers(otherUserSessions);
        await Clients.OthersInGroup(meetingNumber).OtherJoined(userSession);
    }

    public async Task GetMeetingInfoAsync(bool includeUserSession = true)
    {
        var meetingResponse = await _meetingService
            .GetMeetingByNumberAsync(new GetMeetingByNumberRequest { MeetingNumber = meetingNumber, IncludeUserSession = includeUserSession}).ConfigureAwait(false);
        
        await Clients.All.SendAsync("GetMeetingInfoAsync", meetingResponse.Data);
    }
    
    public async Task SendMessageAsync(string message)
    {
        await Clients.Caller.SendAsync("SendMessageAsync", message);
    }

    public async Task DrawOnCanvasAsync(string drawingData)
    {
        var userSession = await _meetingDataProvider.GetUserSessionByStreamIdAsync(streamId).ConfigureAwait(false);
        
        if (userSession != null)
            await Clients.OthersInGroup(meetingNumber).CanvasDrawingReceived(userSession.UserId, drawingData);
    }
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userSession = await _meetingDataProvider.GetUserSessionByStreamIdAsync(streamId).ConfigureAwait(false);
        
        if (userSession != null)
            await Clients.OthersInGroup(meetingNumber).OtherLeft(userSession);
        
        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }

    public async Task ConnectionsClosedAsync(IEnumerable<string> peerConnectionIds)
    {
        await _meetingService.EndMeetingAsync(new EndMeetingCommand { MeetingNumber = meetingNumber }).ConfigureAwait(false);
        
        await Clients.OthersInGroup(meetingNumber).OtherConnectionsClosed(peerConnectionIds);
    }
}
