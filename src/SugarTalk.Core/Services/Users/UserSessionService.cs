using System;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver.Linq;
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

        Task<UserSessionWebRtcConnectionRemovedEvent> RemoveUserSessionWebRtcConnection(
            RemoveUserSessionWebRtcConnectionCommand command, CancellationToken cancellationToken);
        
        Task<UserSessionWebRtcConnectionStatusUpdatedEvent> UpdateUserSessionWebRtcConnectionStatus(
            UpdateUserSessionWebRtcConnectionStatusCommand command, CancellationToken cancellationToken);
        
        Task<AudioChangedEvent> ChangeAudio(ChangeAudioCommand changeAudioCommand, CancellationToken cancellationToken = default);

        Task<ScreenSharedEvent> ShareScreen(ShareScreenCommand shareScreenCommand,
            CancellationToken cancellationToken = default);
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

        public async Task AddUserSessionWebRtcConnection(UserSessionWebRtcConnection webRtcConnection,
            CancellationToken cancellationToken = default)
        {
            await _repository.AddAsync(webRtcConnection, cancellationToken).ConfigureAwait(false);
        }

        public async Task<UserSessionWebRtcConnectionRemovedEvent> RemoveUserSessionWebRtcConnection(
            RemoveUserSessionWebRtcConnectionCommand command, CancellationToken cancellationToken)
        {
            var webRtcConnection = await _userSessionDataProvider
                .GetUserSessionWebRtcConnectionById(Guid.Empty, command.WebRtcPeerConnectionId, cancellationToken).ConfigureAwait(false);

            if (webRtcConnection == null)
                throw new ArgumentNullException(nameof(webRtcConnection));
            
            await _repository.RemoveAsync(webRtcConnection, cancellationToken).ConfigureAwait(false);
            
            return new UserSessionWebRtcConnectionRemovedEvent
            {
                RemovedConnection = _mapper.Map<UserSessionWebRtcConnectionDto>(webRtcConnection)
            };
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

        public async Task<ScreenSharedEvent> ShareScreen(ShareScreenCommand shareScreenCommand,
            CancellationToken cancellationToken = default)
        {
            var userSession = await _userSessionDataProvider
                .GetUserSessionById(shareScreenCommand.UserSessionId, cancellationToken).ConfigureAwait(false);

            if (shareScreenCommand.IsShared)
            {
                var otherSharing = await _repository.Query<UserSession>()
                    .Where(x => x.Id != userSession.Id)
                    .Where(x => x.MeetingSessionId == userSession.MeetingSessionId)
                    .AnyAsync(x => x.IsSharingScreen, cancellationToken).ConfigureAwait(false);
                
                if (!otherSharing)
                {
                    userSession.IsSharingScreen = true;
                    await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                userSession.IsSharingScreen = false;
                await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);
            }
            
            return new ScreenSharedEvent
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