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

public interface IUserSessionDataProvider : IScopedDependency
{
    Task<MeetingUserSession> GetUserSessionByIdAsync(int id, CancellationToken cancellationToken);
    
    Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken);

    Task AddUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
        
    Task UpdateUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
    
    Task RemoveUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
    
    Task AddUserSessionStreamAsync(MeetingUserSession userSession, List<string> streamIds, CancellationToken cancellationToken);

    Task RemoveUserSessionStreamsAsync(MeetingUserSession userSession, CancellationToken cancellationToken);
}

public class UserSessionDataProvider : IUserSessionDataProvider
{
    private readonly IMapper _mapper;
    private readonly IRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UserSessionDataProvider(IMapper mapper, IRepository repository, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<MeetingUserSession> GetUserSessionByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingUserSession>()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<MeetingUserSessionDto>> GetUserSessionsByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken)
    {
        var userSessions = await _repository.Query<MeetingUserSession>(x => x.MeetingId == meetingId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        
        return _mapper.Map<List<MeetingUserSessionDto>>(userSessions);
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

    public async Task RemoveUserSessionAsync(MeetingUserSession userSession, CancellationToken cancellationToken)
    {
        if (userSession != null)
            await _repository.DeleteAsync(userSession, cancellationToken).ConfigureAwait(false);
    }

    public async Task AddUserSessionStreamAsync(MeetingUserSession userSession, List<string> streamIds, CancellationToken cancellationToken)
    {
        var meetingUserSessionStreams = new List<MeetingUserSessionStream>();
        
        streamIds.ForEach(streamId =>
        {
            meetingUserSessionStreams.Add(new MeetingUserSessionStream()
            {
                StreamId = streamId,
                UserSessionId = userSession.Id
            });
        });

        await _repository.InsertAllAsync(meetingUserSessionStreams, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveUserSessionStreamsAsync(MeetingUserSession userSession, CancellationToken cancellationToken)
    {
        if (userSession == null) return;

        var meetingUserSessionStreams = await _repository.Query<MeetingUserSessionStream>()
            .Where(x => userSession.Id == x.UserSessionId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        await _repository.DeleteAllAsync(meetingUserSessionStreams, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}