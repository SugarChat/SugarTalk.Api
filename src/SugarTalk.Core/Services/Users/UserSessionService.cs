using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Kurento.NET;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using SugarTalk.Messages.Commands.UserSessions;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Events.UserSessions;

namespace SugarTalk.Core.Services.Users
{
    public interface IUserSessionService
    {
        Task UpdateUserSession(UserSession userSession, CancellationToken cancellationToken = default);
        
        Task RemoveUserSession(UserSession userSession, CancellationToken cancellationToken = default);

        Task UpdateUserSessionEndpoints(Guid userSessionId, WebRtcEndpoint endpoint,
            ConcurrentDictionary<string, WebRtcEndpoint> receivedEndPoints, CancellationToken cancellationToken = default);

        Task<UserSessionConnectionStatusUpdatedEvent> UpdateUserSessionConnectionStatus(UpdateUserSessionConnectionStatusCommand updateStatusCommand, 
            CancellationToken cancellationToken);
        
        Task<AudioChangedEvent> ChangeAudio(ChangeAudioCommand changeAudioCommand, CancellationToken cancellationToken = default);
    }
    
    public class UserSessionService : IUserSessionService
    {
        private readonly IMapper _mapper;
        private readonly IMongoDbRepository _repository;
        private readonly IUserSessionDataProvider _userSessionDataProvider;

        public UserSessionService(IMapper mapper, IMongoDbRepository repository, IUserSessionDataProvider userSessionDataProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _userSessionDataProvider = userSessionDataProvider;
        }
        
        public async Task UpdateUserSession(UserSession userSession, CancellationToken cancellationToken = default)
        {
            await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateUserSessionEndpoints(Guid userSessionId, WebRtcEndpoint endpoint,
            ConcurrentDictionary<string, WebRtcEndpoint> receivedEndPoints, CancellationToken cancellationToken = default)
        {
            var userSession = await _userSessionDataProvider.GetUserSessionById(userSessionId, cancellationToken)
                .ConfigureAwait(false);
            
            if (userSession == null) return;

            if (endpoint != null)
                userSession.WebRtcEndpointId = endpoint.id;
            if (receivedEndPoints != null && receivedEndPoints.Any())
                foreach (var (key, value) in receivedEndPoints)
                    userSession.ReceivedEndPointIds.TryAdd(key, value.id);
            
            await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);
        }
        
        public async Task RemoveUserSession(UserSession userSession, CancellationToken cancellationToken = default)
        {
            if (userSession != null)
                await _repository.RemoveAsync(userSession, cancellationToken).ConfigureAwait(false);
        }

        public async Task<UserSessionConnectionStatusUpdatedEvent> UpdateUserSessionConnectionStatus(UpdateUserSessionConnectionStatusCommand updateStatusCommand, 
            CancellationToken cancellationToken)
        {
            var userSession = await _userSessionDataProvider
                .GetUserSessionByIdOrConnectionId(updateStatusCommand.UserSessionId, updateStatusCommand.ConnectionId, cancellationToken).ConfigureAwait(false);

            userSession.ConnectionStatus = updateStatusCommand.ConnectionStatus;
            
            await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);

            return new UserSessionConnectionStatusUpdatedEvent
            {
                UserSession = _mapper.Map<UserSessionDto>(userSession)
            };
        }
        
        public async Task<AudioChangedEvent> ChangeAudio(ChangeAudioCommand changeAudioCommand, CancellationToken cancellationToken = default)
        {
            var userSession = await _userSessionDataProvider
                .GetUserSessionById(changeAudioCommand.UserSessionId, cancellationToken).ConfigureAwait(false);

            userSession.IsMuted = changeAudioCommand.IsMuted;

            await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);

            return new AudioChangedEvent
            {
                UserSession = _mapper.Map<UserSessionDto>(userSession)
            };
        }
    }
}