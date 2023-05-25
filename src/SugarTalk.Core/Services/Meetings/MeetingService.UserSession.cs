using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<AudioChangedEvent> ChangeAudioAsync(
        ChangeAudioCommand command, CancellationToken cancellationToken);

    Task<ScreenSharedEvent> ShareScreenAsync(
        ShareScreenCommand command, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<AudioChangedEvent> ChangeAudioAsync(ChangeAudioCommand command, CancellationToken cancellationToken)
    {
        var userSession = await _meetingDataProvider
            .GetMeetingUserSessionByIdAsync(command.MeetingUserSessionId, cancellationToken).ConfigureAwait(false);

        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(userSession.MeetingId, cancellationToken).ConfigureAwait(false);

        if (meeting == null) throw new MeetingNotFoundException();

        var response = new ConferenceRoomResponseBaseDto();

        if (command.IsMuted)
        {
            if (userSession.UserId != _currentUser.Id)
                throw new CannotChangeAudioWhenConfirmRequiredException();

            userSession.IsMuted = true;
            
            response = await _antMediaServerUtilService
                .AddStreamToMeetingAsync(appName, meeting.MeetingNumber, command.StreamId, cancellationToken).ConfigureAwait(false);
        }
        else
            userSession.IsMuted = false;

        await _meetingDataProvider.UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);

        return new AudioChangedEvent
        {
            Response = response,
            MeetingUserSession = _mapper.Map<MeetingUserSessionDto>(userSession)
        };
    }

    public async Task<ScreenSharedEvent> ShareScreenAsync(ShareScreenCommand command, CancellationToken cancellationToken)
    {
        var userSession = await _meetingDataProvider
            .GetMeetingUserSessionByIdAsync(command.MeetingUserSessionId, cancellationToken).ConfigureAwait(false);

        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(userSession.MeetingId, cancellationToken).ConfigureAwait(false);

        var response = new ConferenceRoomResponseBaseDto();
        
        if (command.IsShared)
        {
            var otherSharing = await _meetingDataProvider
                .IsOtherSharingAsync(userSession, cancellationToken).ConfigureAwait(false);

            if (!otherSharing && userSession.UserId == _currentUser.Id)
            {
                userSession.IsSharingScreen = true;

                response = await _antMediaServerUtilService
                    .AddStreamToMeetingAsync(appName, meeting.MeetingNumber, command.StreamId, cancellationToken).ConfigureAwait(false);
            }
        }
        else
            userSession.IsSharingScreen = false;

        await _meetingDataProvider
            .UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
        
        return new ScreenSharedEvent
        {
            Response = response,
            MeetingUserSession = _mapper.Map<MeetingUserSessionDto>(userSession)
        };
    }
}