using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages.Commands.UserSessions;

namespace SugarTalk.Core.Handlers.CommandHandlers.UserSessions
{
    public class RemoveUserSessionWebRtcConnectionCommandHandler : ICommandHandler<RemoveUserSessionWebRtcConnectionCommand>
    {
        private readonly IUserSessionService _userSessionService;

        public RemoveUserSessionWebRtcConnectionCommandHandler(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        public async Task Handle(IReceiveContext<RemoveUserSessionWebRtcConnectionCommand> context, CancellationToken cancellationToken)
        {
            var removedEvent = await _userSessionService
                .RemoveUserSessionWebRtcConnection(context.Message, cancellationToken).ConfigureAwait(false);

            await context.PublishAsync(removedEvent, cancellationToken).ConfigureAwait(false);
        }
    }
}