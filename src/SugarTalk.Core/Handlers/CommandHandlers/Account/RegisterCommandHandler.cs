using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Account;
using SugarTalk.Messages.Commands.Account;

namespace SugarTalk.Core.Handlers.CommandHandlers.Account;

public class RegisterRequestHandler : ICommandHandler<RegisterCommand, RegisterResponse>
{
    private readonly IAccountService _accountService;

    public RegisterRequestHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<RegisterResponse> Handle(IReceiveContext<RegisterCommand> context, CancellationToken cancellationToken)
    {
        var userAccountRegisteredEvent = await _accountService.RegisterAsync(context.Message, cancellationToken);
        
        await context.PublishAsync(userAccountRegisteredEvent, cancellationToken).ConfigureAwait(false);

        return new RegisterResponse { Data = true };
    }
}