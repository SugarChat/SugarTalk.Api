using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<MeetingUserSession> GetMeetingUserSessionByIdAsync(int id, CancellationToken cancellationToken);
    
    Task AddMeetingUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
        
    Task UpdateMeetingUserSessionAsync(List<MeetingUserSession> userSessions, CancellationToken cancellationToken);

    Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAsync(
        Guid meetingId, Guid? meetingSubId, bool isQueryKickedOut = false, bool? includeUserName = false, CancellationToken cancellationToken = default);
    
    Task RemoveMeetingUserSessionsIfRequiredAsync(int userId, Guid meetingId, CancellationToken cancellationToken);
    
    Task<bool> IsOtherSharingAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
    
    Task<List<MeetingUserSession>> GetMeetingUserSessionsAsync(List<int> ids = null,  Guid? meetingId = null, bool? coHost = null, CancellationToken cancellationToken = default);
    
    Task<MeetingUserSessionDto> GetMeetingUserSessionByUserIdAsync(int userId, CancellationToken cancellationToken);
    
    Task<List<MeetingUserSession>> GetMeetingUserSessionByUserIdsAsync(List<int> userIds, Guid? meetingSubId, CancellationToken cancellationToken);

    Task<List<MeetingUserSession>> GetMeetingUserSessionsByIdsAndMeetingIdAsync(List<int> ids, Guid meetingId, CancellationToken cancellationToken);

    Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAndOnlineTypeAsync(Guid meetingId,CancellationToken cancellationToken);

    Task<MeetingOnlineLongestDurationUserDto> GetMeetingMinJoinUserByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken);

    Task<List<MeetingUserSessionDto>> GetAllMeetingUserSessionsAsync(Guid meetingId, CancellationToken cancellationToken);
}

public partial class MeetingDataProvider
{
    public async Task<MeetingUserSession> GetMeetingUserSessionByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _repository.QueryNoTracking<MeetingUserSession>()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddMeetingUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken)
    {
        if (userSession != null)
        {
            await _repository.InsertAsync(userSession, cancellationToken).ConfigureAwait(false);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task UpdateMeetingUserSessionAsync(List<MeetingUserSession> userSessions, CancellationToken cancellationToken)
    {
        if (userSessions != null)
            await _repository.UpdateAllAsync(userSessions, cancellationToken).ConfigureAwait(false);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAsync(
        Guid meetingId, Guid? meetingSubId, bool isQueryKickedOut = false, bool? includeUserName = false, CancellationToken cancellationToken = default)
    {
        var query = _repository.QueryNoTracking<MeetingUserSession>()
            .Where(x => x.MeetingId == meetingId && !x.IsDeleted);

        if (!isQueryKickedOut)
        {
            query = query.Where(x => x.OnlineType != MeetingUserSessionOnlineType.KickOutMeeting);
        }
        
        if (meetingSubId is not null)
        {
            query = query.Where(x => x.MeetingSubId == meetingSubId);
        }

        var userSessions = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        
        var userSessionDtos = _mapper.Map<List<MeetingUserSessionDto>>(userSessions);
    
        if (includeUserName == true)
            await EnrichMeetingUserSessionsByOnlineAsync(userSessionDtos, cancellationToken).ConfigureAwait(false);

        return userSessionDtos;
    }

    public async Task<bool> IsOtherSharingAsync(MeetingUserSession userSession, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingUserSession>()
            .Where(x => x.Id != userSession.Id)
            .Where(x => x.MeetingId == userSession.MeetingId)
            .AnyAsync(x => x.IsSharingScreen, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingUserSession>> GetMeetingUserSessionsAsync(List<int> ids = null, Guid? meetingId = null, bool? coHost = null, CancellationToken cancellationToken = default)
    {
        var query = _repository.Query<MeetingUserSession>();

        if (ids is { Count: > 0 })
            query = query.Where(x => ids.Contains(x.Id));

        if (meetingId.HasValue)
            query = query.Where(x => x.MeetingId == meetingId.Value);

        if (coHost.HasValue)
            query = query.Where(x => x.CoHost == coHost.Value);
        
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<MeetingUserSessionDto> GetMeetingUserSessionByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        return await (from userSession in _repository.QueryNoTracking<MeetingUserSession>().Where(x => x.UserId == userId)
            join meeting in _repository.Query<Meeting>() on userSession.MeetingId equals meeting.Id
            orderby userSession.CreatedDate descending
            select new MeetingUserSessionDto
            {
                Id = userSession.Id,
                UserId = userSession.UserId,
                IsMuted = userSession.IsMuted,
                GuestName = userSession.GuestName,
                MeetingId = userSession.MeetingId,
                OnlineType = userSession.OnlineType,
                CreatedDate = userSession.CreatedDate,
                LastJoinTime = userSession.LastJoinTime,
                MeetingSubId = userSession.MeetingSubId,
                IsSharingScreen = userSession.IsSharingScreen,
                IsMeetingMaster = meeting.MeetingMasterUserId == userSession.UserId
            }).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingUserSession>> GetMeetingUserSessionByUserIdsAsync(List<int> userIds, Guid? meetingSubId, CancellationToken cancellationToken)
    {
        var query = _repository.QueryNoTracking<MeetingUserSession>(x => userIds.Contains(x.UserId));
        
        if (meetingSubId is not null)
        {
            query = query.Where(x => x.MeetingSubId == meetingSubId);
        }

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveMeetingUserSessionsIfRequiredAsync(int userId, Guid meetingId, CancellationToken cancellationToken)
    {
        var meetingUserSessions = await _repository.QueryNoTracking<MeetingUserSession>()
            .Where(x => x.UserId == userId)
            .Where(x => x.MeetingId != meetingId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (meetingUserSessions is not { Count: > 0 }) return;

        await _repository.DeleteAllAsync(meetingUserSessions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingUserSession>> GetMeetingUserSessionsByIdsAndMeetingIdAsync(List<int> ids,
        Guid meetingId, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingUserSession>()
            .Where(x => ids.Contains(x.Id))
            .Where(x =>
                x.MeetingId == meetingId &&
                x.Status != MeetingAttendeeStatus.Absent &&
                !x.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAndOnlineTypeAsync(Guid meetingId, CancellationToken cancellationToken)
    {
        var userSessions = await _repository.QueryNoTracking<MeetingUserSession>(x => x.MeetingId == meetingId && x.OnlineType == MeetingUserSessionOnlineType.Online)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        
        return _mapper.Map<List<MeetingUserSessionDto>>(userSessions);
    }

    public async Task<MeetingOnlineLongestDurationUserDto> GetMeetingMinJoinUserByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken)
    {
        return await (from session in _repository.Query<MeetingUserSession>().Where(x => x.MeetingId == meetingId && x.OnlineType == MeetingUserSessionOnlineType.Online)
            join userAccount in _repository.Query<UserAccount>() on session.UserId equals userAccount.Id
            orderby session.LastJoinTime
            select new MeetingOnlineLongestDurationUserDto
            {
                UserId = userAccount.Id,
                UserName = userAccount.UserName
            }).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingUserSessionDto>> GetAllMeetingUserSessionsAsync(
        Guid meetingId, CancellationToken cancellationToken)
    {
        return await (from session in _repository.Query<MeetingUserSession>().Where(x => x.MeetingId == meetingId && x.OnlineType == MeetingUserSessionOnlineType.Online)
            join userAccount in _repository.Query<UserAccount>() on session.UserId equals userAccount.Id
            join meeting in _repository.Query<Meeting>() on session.MeetingId equals meeting.Id
            orderby session.LastJoinTime
            select new MeetingUserSessionDto
            {
                Id = session.Id,
                UserId = session.UserId,
                CoHost = session.CoHost,
                IsMuted = session.IsMuted,
                GuestName = session.GuestName,
                MeetingId = session.MeetingId,
                UserName = userAccount.UserName,
                OnlineType = session.OnlineType,
                CreatedDate = session.CreatedDate,
                LastJoinTime = session.LastJoinTime,
                MeetingSubId = session.MeetingSubId,
                IsSharingScreen = session.IsSharingScreen,
                LastModifiedDateForCoHost = session.LastModifiedDateForCoHost,
                IsMeetingMaster = meeting.MeetingMasterUserId == userAccount.Id,
                IsMeetingCreator = session.UserId == meeting.CreatedBy
            }).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
