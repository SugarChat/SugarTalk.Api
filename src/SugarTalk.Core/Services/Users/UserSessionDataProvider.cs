using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Core.Services.Users
{
    public interface IUserSessionDataProvider
    {
        Task<UserSession> GetUserSessionById(Guid id, CancellationToken cancellationToken = default);
        
        Task<UserSessionDto> GetUserSessionDtoById(Guid id, CancellationToken cancellationToken = default);

        Task<List<UserSessionDto>> GetUserSessionsByMeetingSessionId(Guid meetingSessionId, CancellationToken cancellationToken = default);

        Task<UserSession> GetUserSessionByConnectionId(string connectionId,
            CancellationToken cancellationToken = default);
    }
    
    public class UserSessionDataProvider : IUserSessionDataProvider
    {
        private readonly IMapper _mapper;
        private readonly IRepository _repository;

        public UserSessionDataProvider(IMapper mapper, IRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<UserSession> GetUserSessionById(Guid id, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<UserSession>()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserSessionDto> GetUserSessionDtoById(Guid id, CancellationToken cancellationToken = default)
        {
            var userSession = await _repository.Query<UserSession>()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

            return _mapper.Map<UserSessionDto>(userSession);
        }
        
        public async Task<UserSession> GetUserSessionByConnectionId(string connectionId, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<UserSession>()
                .FirstOrDefaultAsync(x => x.ConnectionId == connectionId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<UserSessionDto>> GetUserSessionsByMeetingSessionId(Guid meetingSessionId, CancellationToken cancellationToken = default)
        {
            var userSessions = await _repository.Query<UserSession>(x => x.MeetingSessionId == meetingSessionId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            return _mapper.Map<List<UserSessionDto>>(userSessions);
        }
    }
}