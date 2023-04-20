using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Account;
using SugarTalk.Messages.Requests.Account;

namespace SugarTalk.Core.Handlers.RequestHandlers.Account
{
    public class SignInFromThirdPartyRequestHandler : IRequestHandler<SignInFromThirdPartyRequest, SignInFromThirdPartyResponse>
    {
        private readonly IAccountService _userService;

        public SignInFromThirdPartyRequestHandler(IAccountService userService)
        {
            _userService = userService;
        }

        public async Task<SignInFromThirdPartyResponse> Handle(IReceiveContext<SignInFromThirdPartyRequest> context, CancellationToken cancellationToken)
        {
            return await _userService.SignInFromThirdParty(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}