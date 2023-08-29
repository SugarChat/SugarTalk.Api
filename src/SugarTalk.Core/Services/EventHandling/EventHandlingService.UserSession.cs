using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Services.EventHandling;

public partial class EventHandlingService
{
    public async Task HandlingEventAsync(AudioChangedEvent @event, CancellationToken cancellationToken)
    {
        if (@event?.MeetingUserSession == null) return;

        var meeting = await _meetingDataProvider
            .GetMeetingByIdAsync(@event.MeetingUserSession.MeetingId, cancellationToken).ConfigureAwait(false);

        await _meetingHub.Clients
            .GroupExcept(meeting.MeetingNumber, @event.MeetingUserSession.UserSessionStreams.FirstOrDefault().StreamId)
            .SendAsync("OtherAudioChangedAsync", @event.MeetingUserSession, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandlingEventAsync(ScreenSharedEvent @event, CancellationToken cancellationToken)
    {
        if (@event?.MeetingUserSession.UserSessionStreams is not { Count: > 0 }) return;
        
        var meeting = await _meetingDataProvider
            .GetMeetingByIdAsync(@event.MeetingUserSession.MeetingId, cancellationToken).ConfigureAwait(false);

        await _meetingHub.Clients
            .GroupExcept(meeting.MeetingNumber, @event.MeetingUserSession.UserSessionStreams.FirstOrDefault().StreamId)
            .SendAsync("OtherScreenSharedAsync", @event.MeetingUserSession, cancellationToken).ConfigureAwait(false);
    }
}