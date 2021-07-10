using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Authentication;
using SugarTalk.Messages.Requests.Authentication;

namespace SugarTalk.Core.Handlers.RequestHandlers.Authentication
{
    public class GetGoogleAccessTokenRequestHandler : IRequestHandler<GetGoogleAccessTokenRequest, GetGoogleAccessTokenResponse>
    {
        private readonly IAuthenticationService _authenticationService;

        public GetGoogleAccessTokenRequestHandler(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<GetGoogleAccessTokenResponse> Handle(IReceiveContext<GetGoogleAccessTokenRequest> context, CancellationToken cancellationToken)
        {
            return await _authenticationService.GetGoogleAccessToken(context.Message, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}