using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages.Commands.UserSessions;

namespace SugarTalk.Core.Handlers.CommandHandlers.UserSessions
{
    public class ChangeAudioCommandHandler : ICommandHandler<ChangeAudioCommand>
    {
        private readonly IUserSessionService _userSessionService;

        public ChangeAudioCommandHandler(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        public async Task Handle(IReceiveContext<ChangeAudioCommand> context, CancellationToken cancellationToken)
        {
            var audioChangedEvent = await _userSessionService.ChangeAudio(context.Message, cancellationToken)
                .ConfigureAwait(false);

            await context.PublishAsync(audioChangedEvent, cancellationToken).ConfigureAwait(false);
        }
    }
}