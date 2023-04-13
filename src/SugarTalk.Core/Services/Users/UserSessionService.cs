using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Messages.Commands.UserSessions;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Events.UserSessions;

namespace SugarTalk.Core.Services.Users
{
    public interface IUserSessionService
    {
        Task UpdateUserSession(UserSession userSession, CancellationToken cancellationToken = default);
        
        Task RemoveUserSession(UserSession userSession, CancellationToken cancellationToken = default);

        Task<AudioChangedEvent> ChangeAudio(ChangeAudioCommand changeAudioCommand, CancellationToken cancellationToken = default);

        Task<ScreenSharedEvent> ShareScreen(ShareScreenCommand shareScreenCommand,
            CancellationToken cancellationToken = default);
    }
    
    public class UserSessionService : IUserSessionService
    {
        private readonly IMapper _mapper;
        private readonly IRepository _repository;
        private readonly IUserSessionDataProvider _userSessionDataProvider;

        public UserSessionService(IMapper mapper, IRepository repository, IUserSessionDataProvider userSessionDataProvider)
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
                await _repository.DeleteAsync(userSession, cancellationToken).ConfigureAwait(false);
            }
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