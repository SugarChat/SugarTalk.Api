using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Authentication;
using SugarTalk.Messages.Requests.Authentication;

namespace SugarTalk.Core.Handlers.RequestHandlers.Authentication
{
    public class GetFacebookAccessTokenRequestHandler : IRequestHandler<GetFacebookAccessTokenRequest, GetFacebookAccessTokenResponse>
    {
        private readonly IAuthenticationService _authenticationService;

        public GetFacebookAccessTokenRequestHandler(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<GetFacebookAccessTokenResponse> Handle(IReceiveContext<GetFacebookAccessTokenRequest> context, CancellationToken cancellationToken)
        {
            return await _authenticationService.GetFacebookAccessToken(context.Message, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}