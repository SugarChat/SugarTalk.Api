using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Account;
using SugarTalk.Messages.Requests.Account;

namespace SugarTalk.Core.Handlers.RequestHandlers.Account;

public class LoginRequestHandler : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly IAccountService _accountService;

    public LoginRequestHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<LoginResponse> Handle(IReceiveContext<LoginRequest> context, CancellationToken cancellationToken)
    {
        return await _accountService.LoginAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}