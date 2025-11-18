using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Account;
using SugarTalk.Messages.Requests.Account;

namespace SugarTalk.Core.Handlers.CommandHandlers.Account;

public class UploadPhotoCommandHandler : ICommandHandler<UploadPhotoCommand, UploadPhotoResponse>
{
    private readonly IAccountService _accountService;

    public UploadPhotoCommandHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }
    
    public async Task<UploadPhotoResponse> Handle(IReceiveContext<UploadPhotoCommand> context, CancellationToken cancellationToken)
    {
        return await _accountService.UploadPhotoAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}