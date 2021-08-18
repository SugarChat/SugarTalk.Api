using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Kurento.NET;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Core.Services.Users
{
    public interface IUserSessionDataProvider
    {
        Task<UserSession> GetUserSessionById(Guid id, CancellationToken cancellationToken = default);
        
        Task<UserSessionDto> GetUserSessionById(Guid id, bool includeWebRtcConnections = false, CancellationToken cancellationToken = default);

        Task<List<UserSessionDto>> GetUserSessionsByMeetingSessionId(Guid meetingSessionId, CancellationToken cancellationToken = default);

        Task<UserSession> GetUserSessionByConnectionId(string connectionId,
            CancellationToken cancellationToken = default);

        Task<UserSessionWebRtcConnection> GetUserSessionWebRtcConnectionById(Guid id,
            CancellationToken cancellationToken = default);
        
        Task<List<UserSessionWebRtcConnection>> GetUserSessionWebRtcConnectionsByUserSessionId(Guid userSessionId,
            CancellationToken cancellationToken = default);
        
        Task<List<UserSessionWebRtcConnection>> GetUserSessionWebRtcConnectionsByUserSessionIds(IEnumerable<Guid> userSessionIds,
            CancellationToken cancellationToken = default);
    }
    
    public class UserSessionDataProvider : IUserSessionDataProvider
    {
        private readonly IMapper _mapper;
        private readonly KurentoClient _client;
        private readonly IMongoDbRepository _repository;

        public UserSessionDataProvider(IMapper mapper, KurentoClient client, IMongoDbRepository repository)
        {
            _mapper = mapper;
            _client = client;
            _repository = repository;
        }

        public async Task<UserSession> GetUserSessionById(Guid id, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<UserSession>()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserSession> GetUserSessionByConnectionId(string connectionId, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<UserSession>()
                .FirstOrDefaultAsync(x => x.ConnectionId == connectionId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserSessionWebRtcConnection> GetUserSessionWebRtcConnectionById(Guid id, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<UserSessionWebRtcConnection>()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<UserSessionWebRtcConnection>> GetUserSessionWebRtcConnectionsByUserSessionId(Guid userSessionId, CancellationToken cancellationToken = default)
        {
            return await _repository.Query<UserSessionWebRtcConnection>()
                .Where(x => x.UserSessionId == userSessionId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<UserSessionWebRtcConnection>> GetUserSessionWebRtcConnectionsByUserSessionIds(IEnumerable<Guid> userSessionIds,
            CancellationToken cancellationToken = default)
        {
            return await _repository.Query<UserSessionWebRtcConnection>()
                .Where(x => userSessionIds.Contains(x.UserSessionId))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<UserSessionDto>> GetUserSessionsByMeetingSessionId(Guid meetingSessionId, CancellationToken cancellationToken = default)
        {
            var userSessions = await _repository.Query<UserSession>()
                .Where(x => x.MeetingSessionId == meetingSessionId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var userSessionIds = userSessions.Select(x => x.Id).ToList();

            var userSessionWebRtcConnections = await _repository.Query<UserSessionWebRtcConnection>()
                .Where(x => userSessionIds.Contains(x.UserSessionId))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            
            var allUserSessions = _mapper.Map<List<UserSessionDto>>(userSessions);

            allUserSessions.ForEach(userSession =>
            {
                EnrichUserSession(userSession, userSessionWebRtcConnections);
            });
            
            return allUserSessions;
        }

        public async Task<UserSessionDto> GetUserSessionById(Guid id, bool includeWebRtcConnections = false, CancellationToken cancellationToken = default)
        {
            var userSession = await _repository.Query<UserSession>()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

            if (userSession != null)
            {
                var userSessionResult = _mapper.Map<UserSessionDto>(userSession);

                if (includeWebRtcConnections)
                {
                    var userSessionWebRtcConnections =
                        await GetUserSessionWebRtcConnectionsByUserSessionId(userSessionResult.Id, cancellationToken)
                            .ConfigureAwait(false);

                    EnrichUserSession(userSessionResult, userSessionWebRtcConnections);
                }

                return userSessionResult;
            }

            return null;
        }

        private void EnrichUserSession(UserSessionDto userSession, IEnumerable<UserSessionWebRtcConnection> webRtcConnections)
        {
            var userSessionWebRtcConnections =
                webRtcConnections.Where(x => x.UserSessionId == userSession.Id)
                    .Select(x => _mapper.Map<UserSessionWebRtcConnectionDto>(x)).ToList();
            userSessionWebRtcConnections.ForEach(connection => connection.WebRtcEndpoint = GetEndpointById(connection.WebRtcEndpointId));
            userSession.WebRtcConnections = userSessionWebRtcConnections;
        }
        
        private WebRtcEndpoint GetEndpointById(string endpointId)
        {
            if (string.IsNullOrEmpty(endpointId)) return null;
            
            return _client.GetObjectById(endpointId) as WebRtcEndpoint;
        }
    }
}