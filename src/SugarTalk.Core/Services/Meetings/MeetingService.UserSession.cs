using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<AudioChangedEvent> ChangeAudioAsync(
        ChangeAudioCommand command, CancellationToken cancellationToken);

    Task<ScreenSharedEvent> ShareScreenAsync(
        ShareScreenCommand command, CancellationToken cancellationToken);

    Task<GetMeetingUserSessionsResponse> GetMeetingUserSessionsAsync(
        GetMeetingUserSessionsRequest request, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<AudioChangedEvent> ChangeAudioAsync(ChangeAudioCommand command, CancellationToken cancellationToken)
    {
        var userSession = await _meetingDataProvider
            .GetMeetingUserSessionByIdAsync(command.MeetingUserSessionId, cancellationToken).ConfigureAwait(false);

        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(userSession.MeetingId, cancellationToken).ConfigureAwait(false);

        if (meeting == null) throw new MeetingNotFoundException();

        if (command.IsMuted && userSession.UserId != _currentUser.Id) throw new CannotChangeAudioWhenConfirmRequiredException();

        userSession.IsMuted = command.IsMuted;

        await _meetingDataProvider.UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);

        var updateMeeting = await _meetingDataProvider.GetMeetingAsync(meeting.MeetingNumber, cancellationToken).ConfigureAwait(false);

        return new AudioChangedEvent
        {
            MeetingUserSession = updateMeeting.UserSessions.FirstOrDefault(x => x.Id == userSession.Id)
        };
    }

    public async Task<ScreenSharedEvent> ShareScreenAsync(ShareScreenCommand command, CancellationToken cancellationToken)
    {
        var userSession = await _meetingDataProvider
            .GetMeetingUserSessionByIdAsync(command.MeetingUserSessionId, cancellationToken).ConfigureAwait(false);

        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(userSession.MeetingId, cancellationToken).ConfigureAwait(false);
        
        if (meeting == null) throw new MeetingNotFoundException();

        if (command.IsShared)
        {
            var otherSharing = await _meetingDataProvider
                .IsOtherSharingAsync(userSession, cancellationToken).ConfigureAwait(false);

            if (!otherSharing && userSession.UserId == _currentUser.Id)
            {
                userSession.IsSharingScreen = true;
                
                await AddMeetingUserSessionStreamAsync(
                    userSession.Id, command.StreamId, MeetingStreamType.ScreenSharing, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            userSession.IsSharingScreen = false;

            await RemoveMeetingUserSessionStreamAsync(userSession.Id, MeetingStreamType.ScreenSharing, cancellationToken).ConfigureAwait(false);
        }

        await _meetingDataProvider
            .UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
        
        var updateMeeting = await _meetingDataProvider.GetMeetingAsync(meeting.MeetingNumber, cancellationToken).ConfigureAwait(false);

        return new ScreenSharedEvent
        {
            MeetingUserSession = updateMeeting.UserSessions.FirstOrDefault(x => x.Id == userSession.Id)
        };
    }
    
    public async Task<GetMeetingUserSessionsResponse> GetMeetingUserSessionsAsync(GetMeetingUserSessionsRequest request, CancellationToken cancellationToken)
    {
        var meetingUserSessions = await _meetingDataProvider
            .GetMeetingUserSessionsAsync(request.Ids, cancellationToken).ConfigureAwait(false);

        return new GetMeetingUserSessionsResponse
        {
            Data = _mapper.Map<List<MeetingUserSessionDto>>(meetingUserSessions)
        };
    }
    
    private async Task AddMeetingUserSessionStreamAsync(
        int userSessionId, string streamId, MeetingStreamType streamType, CancellationToken cancellationToken)
    {
        var userSessionStream = new MeetingUserSessionStream
        {
            StreamId = streamId,
            StreamType = streamType,
            MeetingUserSessionId = userSessionId
        };

        var userSessionStreams = 
            await _meetingDataProvider.GetMeetingUserSessionStreamsAsync(userSessionId, cancellationToken).ConfigureAwait(false);

        if (userSessionStreams.Any(x => x.StreamType == streamType))
            throw new CannotAddStreamWhenStreamTypeExistException(streamType);
        
        await _meetingDataProvider
            .AddMeetingUserSessionStreamAsync(userSessionStream, cancellationToken).ConfigureAwait(false);
    }

    private async Task RemoveMeetingUserSessionStreamAsync(
        int userSessionId, MeetingStreamType streamType, CancellationToken cancellationToken)
    {
        var userSessionStreams = await _meetingDataProvider
            .GetMeetingUserSessionStreamsAsync(userSessionId, cancellationToken).ConfigureAwait(false);

        await _meetingDataProvider.RemoveMeetingUserSessionStreamsAsync(
            userSessionStreams.Where(x => x.StreamType == streamType).ToList(), cancellationToken).ConfigureAwait(false);
    }
}