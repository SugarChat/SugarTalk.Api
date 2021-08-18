using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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

        Task AddUserSessionWebRtcConnection(UserSessionWebRtcConnection webRtcConnection,
            CancellationToken cancellationToken = default);
        
        Task<UserSessionWebRtcConnectionStatusUpdatedEvent> UpdateUserSessionWebRtcConnectionStatus(
            UpdateUserSessionWebRtcConnectionStatusCommand command, CancellationToken cancellationToken);
        
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
        
        public async Task RemoveUserSession(UserSession userSession, CancellationToken cancellationToken = default)
        {
            if (userSession != null)
            {
                var userSessionWebRtcConnections = await _userSessionDataProvider
                    .GetUserSessionWebRtcConnectionsByUserSessionId(userSession.Id, cancellationToken).ConfigureAwait(false);
                
                await _repository.RemoveAsync(userSession, cancellationToken).ConfigureAwait(false);
                await _repository.RemoveRangeAsync(userSessionWebRtcConnections, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<UserSessionWebRtcConnectionStatusUpdatedEvent> UpdateUserSessionWebRtcConnectionStatus(
            UpdateUserSessionWebRtcConnectionStatusCommand command, CancellationToken cancellationToken)
        {
            var webRtcConnection = await _userSessionDataProvider
                .GetUserSessionWebRtcConnectionById(command.UserSessionWebRtcConnectionId, cancellationToken).ConfigureAwait(false);

            webRtcConnection.ConnectionStatus = command.ConnectionStatus;
            
            await _repository.UpdateAsync(webRtcConnection, cancellationToken).ConfigureAwait(false);

            return new UserSessionWebRtcConnectionStatusUpdatedEvent
            {
                UserSession = await _userSessionDataProvider
                    .GetUserSessionById(webRtcConnection.UserSessionId, true, cancellationToken).ConfigureAwait(false)
            };
        }

        public async Task AddUserSessionWebRtcConnection(UserSessionWebRtcConnection webRtcConnection,
            CancellationToken cancellationToken = default)
        {
            await _repository.AddAsync(webRtcConnection, cancellationToken).ConfigureAwait(false);
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