using System.Threading;
using Mediator.Net.Context;
using System.Threading.Tasks;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Account;
using SugarTalk.Messages.Requests.Account;

namespace SugarTalk.Core.Handlers.RequestHandlers.Account;

public class GetCurrentUserRequestHandler : IRequestHandler<GetCurrentUserRequest, GetCurrentUserResponse>
{
    private readonly IAccountService _accountService;

    public GetCurrentUserRequestHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<GetCurrentUserResponse> Handle(IReceiveContext<GetCurrentUserRequest> context, CancellationToken cancellationToken)
    {
        return await _accountService.GetCurrentUserAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}