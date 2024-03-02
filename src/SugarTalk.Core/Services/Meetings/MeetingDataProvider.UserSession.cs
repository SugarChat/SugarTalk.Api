using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<MeetingUserSession> GetMeetingUserSessionByIdAsync(int id, CancellationToken cancellationToken);
    
    Task AddMeetingUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
        
    Task UpdateMeetingUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken);

    Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAsync(Guid meetingId, Guid? meetingSubId,
        CancellationToken cancellationToken, bool isQueryKickedOut = false);

    Task RemoveMeetingUserSessionsIfRequiredAsync(int userId, Guid meetingId, CancellationToken cancellationToken);
    
    Task<bool> IsOtherSharingAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
    
    Task<List<MeetingUserSession>> GetMeetingUserSessionsAsync(List<int> ids, CancellationToken cancellationToken);
    
    Task<MeetingUserSession> GetMeetingUserSessionByUserIdAsync(int userId, CancellationToken cancellationToken);
    
    Task<List<MeetingUserSession>> GetMeetingUserSessionByUserIdsAsync(List<int> userIds, Guid? meetingSubId, CancellationToken cancellationToken);

    Task<List<MeetingUserSession>> GetMeetingUserSessionsByIdsAndMeetingIdAsync(List<int> ids, Guid meetingId, CancellationToken cancellationToken);

    Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAndOnlineTypeAsync(Guid meetingId,CancellationToken cancellationToken);
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

    public async Task UpdateMeetingUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken)
    {
        if (userSession != null)
            await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAsync(Guid meetingId, Guid? meetingSubId,
        CancellationToken cancellationToken, bool isQueryKickedOut = false)
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

        return _mapper.Map<List<MeetingUserSessionDto>>(userSessions);
    }

    public async Task<bool> IsOtherSharingAsync(MeetingUserSession userSession, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingUserSession>()
            .Where(x => x.Id != userSession.Id)
            .Where(x => x.MeetingId == userSession.MeetingId)
            .AnyAsync(x => x.IsSharingScreen, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingUserSession>> GetMeetingUserSessionsAsync(List<int> ids, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingUserSession>().Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<MeetingUserSession> GetMeetingUserSessionByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingUserSession>().Where(x => x.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
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
}
