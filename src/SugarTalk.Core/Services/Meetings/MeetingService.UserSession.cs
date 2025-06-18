using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Smarties;
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

    Task<GetMeetingOnlineLongestDurationUserResponse> GetMeetingUserSessionByMeetingIdAsync(
        GetMeetingOnlineLongestDurationUserRequest request, CancellationToken cancellationToken);

    Task<UpdateMeetingUserSessionRoleResponse> UpdateMeetingUserSessionRoleAsync(
        UpdateMeetingUserSessionRoleCommand command, CancellationToken cancellationToken);
    
    Task<GetAllMeetingUserSessionsForMeetingIdResponse> GetAllMeetingUserSessionByMeetingIdAsync(
        GetAllMeetingUserSessionsForMeetingIdRequest request, CancellationToken cancellationToken);

    Task<CheckRenamePermissionResponse> CheckRenamePermissionAsync(CheckRenamePermissionCommand command,
        CancellationToken cancellationToken);

    Task<UpdateMeetingUserSessionTypeResponse> UpdateMeetingUserSessionTypAsync(UpdateMeetingUserSessionTypeCommand command, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<AudioChangedEvent> ChangeAudioAsync(ChangeAudioCommand command, CancellationToken cancellationToken)
    {
        var userSession = await _meetingDataProvider
            .GetMeetingUserSessionByIdAsync(command.MeetingUserSessionId, cancellationToken).ConfigureAwait(false);

        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(userSession.MeetingId, cancellationToken:cancellationToken).ConfigureAwait(false);

        if (meeting == null) throw new MeetingNotFoundException();

        if (command.IsMuted && userSession.UserId != _currentUser.Id) throw new CannotChangeAudioWhenConfirmRequiredException();

        userSession.IsMuted = command.IsMuted;

        await _meetingDataProvider.UpdateMeetingUserSessionAsync(new List<MeetingUserSession>{ userSession }, cancellationToken).ConfigureAwait(false);

        var updateMeeting = await _meetingDataProvider.GetMeetingAsync(meeting.MeetingNumber, cancellationToken: cancellationToken).ConfigureAwait(false);

        return new AudioChangedEvent
        {
            MeetingUserSession = updateMeeting.UserSessions.FirstOrDefault(x => x.Id == userSession.Id)
        };
    }

    public async Task<ScreenSharedEvent> ShareScreenAsync(ShareScreenCommand command, CancellationToken cancellationToken)
    {
        var userSession = await _meetingDataProvider
            .GetMeetingUserSessionByIdAsync(command.MeetingUserSessionId, cancellationToken).ConfigureAwait(false);

        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(userSession.MeetingId, cancellationToken:cancellationToken).ConfigureAwait(false);
        
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
            .UpdateMeetingUserSessionAsync(new List<MeetingUserSession>{ userSession }, cancellationToken).ConfigureAwait(false);
        
        var updateMeeting = await _meetingDataProvider.GetMeetingAsync(meeting.MeetingNumber, cancellationToken: cancellationToken).ConfigureAwait(false);

        return new ScreenSharedEvent
        {
            MeetingUserSession = updateMeeting.UserSessions.FirstOrDefault(x => x.Id == userSession.Id)
        };
    }
    
    public async Task<GetMeetingUserSessionsResponse> GetMeetingUserSessionsAsync(GetMeetingUserSessionsRequest request, CancellationToken cancellationToken)
    {
        var meetingUserSessions = await _meetingDataProvider
            .GetMeetingUserSessionsAsync(request.Ids, cancellationToken: cancellationToken).ConfigureAwait(false);

        return new GetMeetingUserSessionsResponse
        {
            Data = _mapper.Map<List<MeetingUserSessionDto>>(meetingUserSessions)
        };
    }
    
    public async Task<GetMeetingUserSessionByUserIdResponse> GetMeetingUserSessionByUserIdAsync(
        GetMeetingUserSessionByUserIdRequest request, CancellationToken cancellationToken)
    {
        var user = await _accountDataProvider.GetUserAccountAsync(request.UserId != 0 ? request.UserId : _currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (user is null) throw new UnauthorizedAccessException();
        
        var userSession = await _meetingDataProvider.GetMeetingUserSessionByUserIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);

        userSession.IsEntryMeeting = true;
        userSession.OnlineType = MeetingUserSessionOnlineType.Online;
        await _meetingDataProvider.UpdateMeetingUserSessionAsync(new List<MeetingUserSession>{ _mapper.Map<MeetingUserSession>(userSession) }, cancellationToken).ConfigureAwait(false);
        
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
        
        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(request.MeetingId, cancellationToken:cancellationToken).ConfigureAwait(false);
        if (meeting == null) { throw new MeetingNotFoundException(); }

        var userSession = await _meetingDataProvider
            .GetMeetingUserSessionByMeetingIdAsync(request.MeetingId, null, request.UserId, null, cancellationToken)
            .ConfigureAwait(false);
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
            .GetMeetingByIdAsync(command.MeetingId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (meeting == null) throw new MeetingNotFoundException();
        if (meeting.MeetingMasterUserId != _currentUser.Id) throw new UnauthorizedAccessException();

        var masterUserSession = await _meetingDataProvider
            .GetMeetingUserSessionByMeetingIdAndOnlineTypeAsync(meeting.Id, meeting.MeetingMasterUserId,
                cancellationToken).ConfigureAwait(false);
        if (masterUserSession?.UserId != _currentUser.Id) throw new UnauthorizedAccessException();

        var kickOutUserSession = await _meetingDataProvider
            .GetMeetingUserSessionByMeetingIdAndOnlineTypeAsync(meeting.Id, command.KickOutUserId, cancellationToken)
            .ConfigureAwait(false);
        if (kickOutUserSession is null) throw new MeetingUserSessionNotFoundException();
        if (kickOutUserSession.UserId == masterUserSession.UserId) throw new CannotKickOutMeetingUserSessionException();

        kickOutUserSession.OnlineType = MeetingUserSessionOnlineType.KickOutMeeting;
        kickOutUserSession.Status = MeetingAttendeeStatus.Absent;
        await _meetingDataProvider
            .UpdateMeetingUserSessionAsync(new List<MeetingUserSession>{ kickOutUserSession }, cancellationToken).ConfigureAwait(false);
        
        var userSessionOnlineDtos = await _meetingDataProvider
            .GetUserSessionsByMeetingIdAndOnlineTypeAsync(meeting.Id, cancellationToken).ConfigureAwait(false);
        var meetingDto = _mapper.Map<MeetingDto>(meeting);
        meetingDto.UserSessions = userSessionOnlineDtos;

        var user = await _accountDataProvider
            .GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        meetingDto.MeetingTokenFromLiveKit = _liveKitServerUtilService
            .GenerateTokenForJoinMeeting(user, meeting.MeetingNumber);
        return new KickOutMeetingByUserIdResponse() { Data = meetingDto };
    }

    public async Task<GetMeetingOnlineLongestDurationUserResponse> GetMeetingUserSessionByMeetingIdAsync(
        GetMeetingOnlineLongestDurationUserRequest request, CancellationToken cancellationToken)
    {
        var userInfo = await _meetingDataProvider.GetMeetingMinJoinUserByMeetingIdAsync(
            request.MeetingId, cancellationToken).ConfigureAwait(false);

        return new GetMeetingOnlineLongestDurationUserResponse
        {
            Data = userInfo
        };
    }

    public async Task<GetAllMeetingUserSessionsForMeetingIdResponse> GetAllMeetingUserSessionByMeetingIdAsync(
        GetAllMeetingUserSessionsForMeetingIdRequest request, CancellationToken cancellationToken)
    {
        var meetingUserSessions = await _meetingDataProvider.GetAllMeetingUserSessionsAsync(request.MeetingId, cancellationToken).ConfigureAwait(false);
        
        var staffs = await GetMeetingParticipantsAsync(request.MeetingId, cancellationToken).ConfigureAwait(false);

        var noEntryMeetingUsers = staffs.Where(x => !meetingUserSessions.Select(s => s.UserName).ToList().Contains(x.UserName)).ToList();

        var userAccount = new List<UserAccount>();
        
        if (noEntryMeetingUsers.Any())
             userAccount = await _accountDataProvider.GetUserAccountsAsync(userNames: noEntryMeetingUsers.Select(x => x.UserName).ToList(), cancellationToken: cancellationToken).ConfigureAwait(false);
            
        return new GetAllMeetingUserSessionsForMeetingIdResponse
        {
            Data = new GetAllMeetingUserSessionsForMeetingIdDto
            {
                MeetingUserSessions = _mapper.Map<List<MeetingUserSessionDto>>(meetingUserSessions.Where(x => x.OnlineType == MeetingUserSessionOnlineType.Online).ToList()),
                InMeetingCount = meetingUserSessions.Count(x => x.OnlineType == MeetingUserSessionOnlineType.Online),
                WaitingRoomUserSessions = _mapper.Map<List<MeetingUserSessionDto>>(meetingUserSessions.Where(x => x.OnlineType == MeetingUserSessionOnlineType.Waiting).ToList()),
                WaitingRoomCount = meetingUserSessions.Count(x => x.OnlineType == MeetingUserSessionOnlineType.Waiting),
                NoJoinMeetingUsers = _mapper.Map<List<NoJoinMeetingUserSessionsDto>>(userAccount),
                NoEntryMeetingCount = userAccount.Count,
                Count = meetingUserSessions.Count(x => x.IsMeetingMaster || x.CoHost)
            }
        };
    }

    private async Task<List<RmStaffDto>> GetMeetingParticipantsAsync(Guid meetingId, CancellationToken cancellationToken)
    {
        var meetingParticipants  = await _meetingDataProvider.GetMeetingParticipantAsync(new List<Guid>{ meetingId }, cancellationToken: cancellationToken).ConfigureAwait(false);
            
        var participantDict = meetingParticipants.ToDictionary(x => x.StaffId);
            
        Log.Information("Get meeting participant dict: {@participantDict}", participantDict);
            
        var staffs = await _smartiesClient.GetStaffsRequestAsync(new GetStaffsRequestDto
        {
            Ids = participantDict.Keys.ToList()
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("Meeting staffs: {@staffs}", staffs);
        
        return staffs.Data.Staffs;
    }

    public async Task<UpdateMeetingUserSessionRoleResponse> UpdateMeetingUserSessionRoleAsync(
        UpdateMeetingUserSessionRoleCommand command, CancellationToken cancellationToken)
    {
        return command.NewRole switch
        {
            MeetingUserSessionRole.Master => await UpdateMeetingUserSessionRoleForMasterAsync(command, cancellationToken).ConfigureAwait(false),
            MeetingUserSessionRole.CoHost => await UpdateMeetingUserSessionRoleForCoHostAsync(command, cancellationToken).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task<UpdateMeetingUserSessionRoleResponse> UpdateMeetingUserSessionRoleForMasterAsync(UpdateMeetingUserSessionRoleCommand command, CancellationToken cancellationToken)
    {
        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(command.MeetingId, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (meeting.CreatedBy != command.UserId)
            await UpdateMeetingUserSessionCoHostAsync(meeting.Id, meeting.CreatedBy, meeting.MeetingMasterUserId, true, cancellationToken).ConfigureAwait(false);
        else
            await UpdateMeetingUserSessionCoHostAsync(meeting.Id, meeting.CreatedBy, command.UserId, false, cancellationToken).ConfigureAwait(false);

        meeting.MeetingMasterUserId = command.UserId;
        
        await _meetingDataProvider.UpdateMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);
        
        return new UpdateMeetingUserSessionRoleResponse();
    }

    private async Task UpdateMeetingUserSessionCoHostAsync(Guid meetingId, int creatorId, int userId, bool transferMaster, CancellationToken cancellationToken)
    {
        var meetUserSession = await _meetingDataProvider.GetMeetingUserSessionByMeetingIdAsync(meetingId, null, userId, MeetingUserSessionOnlineType.Online, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        meetUserSession.CoHost = transferMaster && meetUserSession.UserId == creatorId;
            
        await _meetingDataProvider.UpdateMeetingUserSessionAsync(new List<MeetingUserSession>{ meetUserSession }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<UpdateMeetingUserSessionRoleResponse> UpdateMeetingUserSessionRoleForCoHostAsync(UpdateMeetingUserSessionRoleCommand command, CancellationToken cancellationToken)
    {
        var meetingUserSession = (await _meetingDataProvider.GetMeetingUserSessionAsync(command.MeetingId, null, command.UserId, sessionOnlineType: MeetingUserSessionOnlineType.Online, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (meetingUserSession == null) throw new MeetingUserSessionNotFoundException();

        meetingUserSession.CoHost = command?.IsCoHost ?? false;
        meetingUserSession.LastModifiedDateForCoHost = _clock.Now;

        await _meetingDataProvider.UpdateMeetingUserSessionAsync(new List<MeetingUserSession>{ meetingUserSession }, cancellationToken).ConfigureAwait(false);
        
        return new UpdateMeetingUserSessionRoleResponse();
    }

    public async Task<CheckRenamePermissionResponse> CheckRenamePermissionAsync(CheckRenamePermissionCommand command, CancellationToken cancellationToken)
    {
        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(command.MeetingId, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        if(meeting == null) throw new MeetingNotFoundException();

        var hasPermission = meeting.MeetingMasterUserId == command.UserId || command.UserId == command.TargetUserId;

        return new CheckRenamePermissionResponse
        {
            Data = new CheckRenamePermissionResponseData
            {
                CanRename = hasPermission
            }
        };
    }

    public async Task<UpdateMeetingUserSessionTypeResponse> UpdateMeetingUserSessionTypAsync(UpdateMeetingUserSessionTypeCommand command, CancellationToken cancellationToken)
    {
        var meetingUserSessions = await _meetingDataProvider.GetMeetingUserSessionsAsync(ids: command.Ids, cancellationToken: cancellationToken);

        if (command.OnlineType.HasValue)
            meetingUserSessions.ForEach(x =>
            {
                if(command.OnlineType == MeetingUserSessionOnlineType.Online)
                    x.IsEntryMeeting = true;
                
                x.OnlineType = command.OnlineType.Value;
            });

        if (command.AllowEntryMeeting.HasValue)
            meetingUserSessions.ForEach(x =>
            {
                x.AllowEntryMeeting = command.AllowEntryMeeting.Value;
                x.OnlineType = MeetingUserSessionOnlineType.Online;
            });
        
        await _meetingDataProvider.UpdateMeetingUserSessionAsync(meetingUserSessions, cancellationToken).ConfigureAwait(false);
        
        return new UpdateMeetingUserSessionTypeResponse
        {
            Data = _mapper.Map<List<MeetingUserSessionDto>>(meetingUserSessions)
        };
    }
}