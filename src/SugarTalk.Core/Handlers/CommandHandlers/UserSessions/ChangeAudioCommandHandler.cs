using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands.UserSessions;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Core.Handlers.CommandHandlers.UserSessions
{
    public class ChangeAudioCommandHandler : ICommandHandler<ChangeAudioCommand, SugarTalkResponse<UserSessionDto>>
    {
        private readonly IUserSessionService _userSessionService;

        public ChangeAudioCommandHandler(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        public async Task<SugarTalkResponse<UserSessionDto>> Handle(IReceiveContext<ChangeAudioCommand> context, CancellationToken cancellationToken)
        {
            var audioChangedEvent = await _userSessionService.ChangeAudio(context.Message, cancellationToken)
                .ConfigureAwait(false);

            await context.PublishAsync(audioChangedEvent, cancellationToken).ConfigureAwait(false);

            return new SugarTalkResponse<UserSessionDto>
            {
                Data = audioChangedEvent.UserSession
            };
        }
    }
}