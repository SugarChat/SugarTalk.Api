using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Requests.Users;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Core.Handlers.RequestHandlers.Users
{
    public class SignInFromThirdPartyRequestHandler : IRequestHandler<SignInFromThirdPartyRequest, SugarTalkResponse<SignedInUserDto>>
    {
        private readonly IUserService _userService;

        public SignInFromThirdPartyRequestHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<SugarTalkResponse<SignedInUserDto>> Handle(IReceiveContext<SignInFromThirdPartyRequest> context, CancellationToken cancellationToken)
        {
            return await _userService.SignInFromThirdParty(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}