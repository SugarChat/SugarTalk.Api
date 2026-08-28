using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Account;
using SugarTalk.Messages.Commands.Account;

namespace SugarTalk.Core.Handlers.CommandHandlers.Account;

public class AddApiKeyPermissionCommandHandler :
    ICommandHandler<AddApiKeyPermissionCommand, AddApiKeyPermissionResponse>
{
    private readonly IAccountDataProvider _accountDataProvider;

    public AddApiKeyPermissionCommandHandler(IAccountDataProvider accountDataProvider)
    {
        _accountDataProvider = accountDataProvider;
    }

    public async Task<AddApiKeyPermissionResponse> Handle(
        IReceiveContext<AddApiKeyPermissionCommand> context, CancellationToken cancellationToken)
    {
        var added = await _accountDataProvider.AddApiKeyPermissionAsync(
            context.Message.ApiKey, context.Message.PermissionName, cancellationToken).ConfigureAwait(false);

        return new AddApiKeyPermissionResponse
        {
            Code = added ? HttpStatusCode.OK : HttpStatusCode.NotFound,
            Data = added
        };
    }
}
