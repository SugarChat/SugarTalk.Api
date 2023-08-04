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
    
    Task AddMeetingUserSessionStreamAsync(MeetingUserSessionStream userSessionStream, CancellationToken cancellationToken);
    
    Task RemoveMeetingUserSessionStreamsAsync(List<int> userSessionIds, CancellationToken cancellationToken = default);
    
    Task<List<MeetingUserSessionStream>> GetMeetingUserSessionStreamsAsync(int userSessionId, CancellationToken cancellationToken);
    
    Task RemoveMeetingUserSessionStreamsAsync(List<MeetingUserSessionStream> userSessionStreams, CancellationToken cancellationToken);

    Task<MeetingUserSession> GetUserSessionByStreamIdAsync(string streamId, CancellationToken cancellationToken = default);
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

    public async Task AddMeetingUserSessionStreamAsync(MeetingUserSessionStream userSessionStream, CancellationToken cancellationToken)
    {
        await _repository.InsertAsync(userSessionStream, cancellationToken).ConfigureAwait(false);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveMeetingUserSessionStreamsAsync(List<int> userSessionIds, CancellationToken cancellationToken)
    {
        var userSessionStreams = await _repository
            .QueryNoTracking<MeetingUserSessionStream>(x => userSessionIds.Contains(x.MeetingUserSessionId))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (userSessionStreams is not { Count: > 0 }) return;

        await _repository.DeleteAllAsync(userSessionStreams, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingUserSessionStream>> GetMeetingUserSessionStreamsAsync(int userSessionId, CancellationToken cancellationToken)
    {
        return await _repository
            .ToListAsync<MeetingUserSessionStream>(
                x => x.MeetingUserSessionId == userSessionId, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveMeetingUserSessionStreamsAsync(List<MeetingUserSessionStream> userSessionStreams, CancellationToken cancellationToken)
    {
        if (userSessionStreams is not { Count: > 0 }) return;

        await _repository.DeleteAllAsync(userSessionStreams, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MeetingUserSession> GetUserSessionByStreamIdAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(streamId)) return null;

        var meetingUserSessionStream = await _repository
            .Query<MeetingUserSessionStream>(x => x.StreamId.Equals(streamId))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return await _repository
            .Query<MeetingUserSession>(x => x.Id == meetingUserSessionStream.MeetingUserSessionId)
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
}
