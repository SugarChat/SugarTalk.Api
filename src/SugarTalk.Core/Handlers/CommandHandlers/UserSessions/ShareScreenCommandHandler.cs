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
    public class ShareScreenCommandHandler : ICommandHandler<ShareScreenCommand, SugarTalkResponse<UserSessionDto>>
    {
        private readonly IUserSessionService _userSessionService;

        public ShareScreenCommandHandler(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        public async Task<SugarTalkResponse<UserSessionDto>> Handle(IReceiveContext<ShareScreenCommand> context, CancellationToken cancellationToken)
        {
            var screenSharedEvent = await _userSessionService.ShareScreen(context.Message, cancellationToken)
                .ConfigureAwait(false);

            await context.PublishAsync(screenSharedEvent, cancellationToken).ConfigureAwait(false);

            return new SugarTalkResponse<UserSessionDto>
            {
                Data = screenSharedEvent.UserSession
            };
        }
    }
}