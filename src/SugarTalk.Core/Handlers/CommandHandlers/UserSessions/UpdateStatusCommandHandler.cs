using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands.UserSessions;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Core.Handlers.CommandHandlers.UserSessions
{
    public class UpdateStatusCommandHandler : ICommandHandler<UpdateStatusCommand, SugarTalkResponse<UserSessionDto>>
    {
        private readonly IUserSessionService _userSessionService;

        public UpdateStatusCommandHandler(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        public async Task<SugarTalkResponse<UserSessionDto>> Handle(IReceiveContext<UpdateStatusCommand> context, CancellationToken cancellationToken)
        {
            var statusUpdatedEvent = await _userSessionService.UpdateStatus(context.Message, cancellationToken)
                .ConfigureAwait(false);

            await context.PublishAsync(statusUpdatedEvent, cancellationToken).ConfigureAwait(false);

            return new SugarTalkResponse<UserSessionDto>
            {
                Data = statusUpdatedEvent.UserSession
            };
        }
    }
}