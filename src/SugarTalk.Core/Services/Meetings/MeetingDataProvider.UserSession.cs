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
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<MeetingUserSession> GetMeetingUserSessionByIdAsync(int id, CancellationToken cancellationToken);
    
    Task AddMeetingUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
        
    Task UpdateMeetingUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
    
    Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken);

    Task RemoveMeetingUserSessionsIfRequiredAsync(int userId, Guid meetingId, CancellationToken cancellationToken);
    
    Task<bool> IsOtherSharingAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
    
    Task<List<MeetingUserSession>> GetMeetingUserSessionsAsync(List<int> ids, CancellationToken cancellationToken);
    
    Task<MeetingUserSession> GetMeetingUserSessionByUserIdAsync(int userId, CancellationToken cancellationToken);

    Task<List<AppointmentMeetingDto>> GetAppointmentMeetingsByUserIdAsync(int userId, CancellationToken cancellationToken);
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
    
    public async Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken)
    {
        var userSessions = await _repository.QueryNoTracking<MeetingUserSession>(x => x.MeetingId == meetingId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        
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

    public async Task RemoveMeetingUserSessionsIfRequiredAsync(int userId, Guid meetingId, CancellationToken cancellationToken)
    {
        var meetingUserSessions = await _repository.QueryNoTracking<MeetingUserSession>()
            .Where(x => x.UserId == userId)
            .Where(x => x.MeetingId != meetingId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (meetingUserSessions is not { Count: > 0 }) return;

        await _repository.DeleteAllAsync(meetingUserSessions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<AppointmentMeetingDto>> GetAppointmentMeetingsByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        var query = 
            from meeting in _repository.Query<Meeting>()
            join rules in _repository.Query<MeetingRepeatRule>()
                on meeting.Id equals rules.MeetingId
            join subMeetings in _repository.Query<MeetingSubMeeting>()
                on meeting.Id equals subMeetings.MeetingId into subMeetingGroup
            from subMeeting in subMeetingGroup.DefaultIfEmpty()
            where meeting.MeetingMasterUserId == userId
            select new AppointmentMeetingDto
            {
                MeetingId = meeting.Id,
                MeetingNumber = meeting.MeetingNumber,
                StartDate = rules.RepeatType == MeetingRepeatType.None ? meeting.StartDate : subMeeting.StartTime,
                EndDate = rules.RepeatType == MeetingRepeatType.None ? meeting.StartDate : subMeeting.EndTime,
                Status = meeting.Status,
                Title = meeting.Title,
                AppointmentType = meeting.AppointmentType
            };
    
        var result = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        if (!result.Any()) return null;

        var sortedResult = result
            .OrderBy(m => (DateTimeOffset.FromUnixTimeSeconds(m.StartDate) - DateTimeOffset.Now).TotalSeconds)
            .ToList();
        
        return sortedResult;
    }
}
