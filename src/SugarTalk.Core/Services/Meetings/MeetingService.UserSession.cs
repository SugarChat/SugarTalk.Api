using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<AudioChangedEvent> ChangeAudioAsync(
        ChangeAudioCommand command, CancellationToken cancellationToken);

    Task<ScreenSharedEvent> ShareScreenAsync(
        ShareScreenCommand command, CancellationToken cancellationToken);

    Task<GetMeetingUserSessionsResponse> GetMeetingUserSessionsAsync(
        GetMeetingUserSessionsRequest request, CancellationToken cancellationToken);

    Task<GetMeetingUserSessionByUserIdResponse> GetMeetingUserSessionByUserIdAsync(
        GetMeetingUserSessionByUserIdRequest request, CancellationToken cancellationToken);
    
    Task<VerifyMeetingUserPermissionResponse> VerifyMeetingUserPermissionAsync(
        VerifyMeetingUserPermissionCommand command, CancellationToken cancellationToken);
    
    Task<KickOutMeetingByUserIdResponse> KickOutMeetingAsync(
        KickOutMeetingByUserIdCommand command, CancellationToken cancellationToken);
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
                
                // await AddMeetingUserSessionStreamAsync(
                //     userSession.Id, command.StreamId, MeetingStreamType.ScreenSharing, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            userSession.IsSharingScreen = false;

            // await RemoveMeetingUserSessionStreamAsync(userSession.Id, MeetingStreamType.ScreenSharing, cancellationToken).ConfigureAwait(false);
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
    
    public async Task<GetMeetingUserSessionByUserIdResponse> GetMeetingUserSessionByUserIdAsync(
        GetMeetingUserSessionByUserIdRequest request, CancellationToken cancellationToken)
    {
        var userSession = await _meetingDataProvider.GetMeetingUserSessionByUserIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);

        var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (user is null) throw new UnauthorizedAccessException();
        
        var userSessionDto = _mapper.Map<MeetingUserSessionDto>(userSession);
        userSessionDto.UserName = user.UserName;
        
        return new GetMeetingUserSessionByUserIdResponse
        {
            Data = userSessionDto
        };
    }
    
    public async Task<VerifyMeetingUserPermissionResponse> VerifyMeetingUserPermissionAsync(
               VerifyMeetingUserPermissionCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != _currentUser.Id) throw new UnauthorizedAccessException();
        
        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(request.MeetingId, cancellationToken).ConfigureAwait(false);
        if (meeting == null) { throw new MeetingNotFoundException(); }
        
        var userSession = await _meetingDataProvider.GetMeetingUserSessionByMeetingIdAsync(request.MeetingId, request.UserId, cancellationToken).ConfigureAwait(false);
        if (userSession is null) throw new MeetingUserSessionNotFoundException();
        
        if (meeting.Id == userSession.MeetingId && meeting.MeetingMasterUserId == userSession.UserId)
        {
            var userSessionDto = _mapper.Map<VerifyMeetingUserPermissionDto>(userSession);
            userSessionDto.IsMeetingMaster = true;
            return new VerifyMeetingUserPermissionResponse() { Data = userSessionDto};
        }
        return new VerifyMeetingUserPermissionResponse() { Data = _mapper.Map<VerifyMeetingUserPermissionDto>(userSession) };
    }

    public async Task<KickOutMeetingByUserIdResponse> KickOutMeetingAsync(KickOutMeetingByUserIdCommand command,
        CancellationToken cancellationToken)
    {
        var meeting = await _meetingDataProvider
            .GetMeetingByIdAsync(command.MeetingId, cancellationToken).ConfigureAwait(false);
        if (meeting == null) throw new MeetingNotFoundException();
        if (meeting.MeetingMasterUserId != _currentUser.Id) throw new UnauthorizedAccessException();

        var masterUserSession = await _meetingDataProvider
            .GetMeetingUserSessionByMeetingIdAndOnlineTypeAsync(meeting.Id, meeting.MeetingMasterUserId,
                cancellationToken).ConfigureAwait(false);
        if (masterUserSession is null) throw new MeetingUserSessionNotFoundException();
        if (masterUserSession.UserId != _currentUser.Id) throw new UnauthorizedAccessException();

        var kickOutUserSession = await _meetingDataProvider
            .GetMeetingUserSessionByMeetingIdAndOnlineTypeAsync(meeting.Id, command.KickOutUserId, cancellationToken)
            .ConfigureAwait(false);
        if (kickOutUserSession is null) throw new MeetingUserSessionNotFoundException();
        if (kickOutUserSession.UserId == masterUserSession.UserId) throw new CannotKickOutMeetingUserSessionException();

        kickOutUserSession.OnlineType = MeetingUserSessionOnlineType.KickOutMeeting;
        kickOutUserSession.Status = MeetingAttendeeStatus.Absent;
        await _meetingDataProvider
            .UpdateMeetingUserSessionAsync(kickOutUserSession, cancellationToken).ConfigureAwait(false);
        
        var userSessionDtos = await _meetingDataProvider
            .GetUserSessionsByMeetingIdAndOnlineTypeAsync(meeting.Id, cancellationToken).ConfigureAwait(false);
        var meetingDto = _mapper.Map<MeetingDto>(meeting);
        meetingDto.UserSessions = userSessionDtos;

        var user = await _accountDataProvider
            .GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        meetingDto.MeetingTokenFromLiveKit = _liveKitServerUtilService
            .GenerateTokenForJoinMeeting(user, meeting.MeetingNumber);
        return new KickOutMeetingByUserIdResponse() { Data = meetingDto };
    }
}