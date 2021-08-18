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
    public class UpdateUserSessionWebRtcConnectionStatusCommandHandler : ICommandHandler<UpdateUserSessionWebRtcConnectionStatusCommand, SugarTalkResponse<UserSessionDto>>
    {
        private readonly IUserSessionService _userSessionService;

        public UpdateUserSessionWebRtcConnectionStatusCommandHandler(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        public async Task<SugarTalkResponse<UserSessionDto>> Handle(
            IReceiveContext<UpdateUserSessionWebRtcConnectionStatusCommand> context, CancellationToken cancellationToken)
        {
            var webRtcConnectionStatusUpdatedEvent = await _userSessionService.UpdateUserSessionWebRtcConnectionStatus(context.Message, cancellationToken)
                .ConfigureAwait(false);

            await context.PublishAsync(webRtcConnectionStatusUpdatedEvent, cancellationToken).ConfigureAwait(false);

            return new SugarTalkResponse<UserSessionDto>
            {
                Data = webRtcConnectionStatusUpdatedEvent.UserSession
            };
        }
    }
}