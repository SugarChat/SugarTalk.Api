using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Messages.Commands.Meetings;
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

        userSession.IsMuted = command.IsMuted;

        await _meetingDataProvider.UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);
        
        //todo: stream

        return new AudioChangedEvent
        {
            MeetingUserSession = _mapper.Map<MeetingUserSessionDto>(userSession)
        };
    }

    public async Task<ScreenSharedEvent> ShareScreenAsync(ShareScreenCommand command,
        CancellationToken cancellationToken)
    {
        var userSession = await _meetingDataProvider
            .GetMeetingUserSessionByIdAsync(command.MeetingUserSessionId, cancellationToken).ConfigureAwait(false);

        if (command.IsShared)
        {
            var otherSharing = await _meetingDataProvider
                .IsOtherSharingAsync(userSession, cancellationToken).ConfigureAwait(false);

            if (!otherSharing)
                userSession.IsSharingScreen = true;
        }
        else
            userSession.IsSharingScreen = false;

        await _meetingDataProvider
            .UpdateMeetingUserSessionAsync(userSession, cancellationToken).ConfigureAwait(false);

        //todo: stream
        
        return new ScreenSharedEvent
        {
            MeetingUserSession = _mapper.Map<MeetingUserSessionDto>(userSession)
        };
    }
}