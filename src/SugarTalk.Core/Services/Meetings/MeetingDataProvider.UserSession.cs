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
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<MeetingUserSession> GetUserSessionByIdAsync(int id, CancellationToken cancellationToken);
    
    Task AddUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
        
    Task UpdateUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
    
    Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken);
    
    Task RemoveMeetingUserSessionsIfRequiredAsync(int userId, Guid meetingId, CancellationToken cancellationToken);
}

public partial class MeetingDataProvider : IMeetingDataProvider
{
    public async Task<MeetingUserSession> GetUserSessionByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingUserSession>()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken)
    {
        if (userSession != null)
        {
            await _repository.InsertAsync(userSession, cancellationToken).ConfigureAwait(false);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task UpdateUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken)
    {
        if (userSession != null)
            await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken)
    {
        var userSessions = await _repository.QueryNoTracking<MeetingUserSession>(x => x.MeetingId == meetingId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        
        return _mapper.Map<List<MeetingUserSessionDto>>(userSessions);
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
}
