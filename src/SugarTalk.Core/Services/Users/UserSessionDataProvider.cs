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
        
        Task<UserSession> GetUserSessionByConnectionId(string connectionId,
            CancellationToken cancellationToken = default);
        
        Task<List<UserSessionDto>> GetUserSessions(Guid meetingSessionId, CancellationToken cancellationToken = default);
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

        public async Task<List<UserSessionDto>> GetUserSessions(Guid meetingSessionId, CancellationToken cancellationToken = default)
        {
            var userSessions = await _repository.Query<UserSession>()
                .Where(x => x.MeetingSessionId == meetingSessionId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            
            var allUserSessions = _mapper.Map<List<UserSessionDto>>(userSessions);

            allUserSessions.ForEach(userSession =>
            {
                if (!string.IsNullOrEmpty(userSession.WebRtcEndpointId))
                    userSession.SendEndPoint = GetEndpointById(userSession.WebRtcEndpointId);
                if (userSession.ReceivedEndPointIds.Any())
                    foreach (var (key, value) in userSession.ReceivedEndPointIds)
                        userSession.ReceivedEndPoints.TryAdd(key, GetEndpointById(value));
            });
            
            return allUserSessions;
        }

        private WebRtcEndpoint GetEndpointById(string endpointId)
        {
            if (string.IsNullOrEmpty(endpointId)) return null;
            
            return _client.GetObjectById(endpointId) as WebRtcEndpoint;
        }
    }
}