using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class CheckRenamePermissionCommandHandler :ICommandHandler<CheckRenamePermissionCommand, CheckRenamePermissionResponse>
{
    private readonly IMeetingService _meetingService;

    public CheckRenamePermissionCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<CheckRenamePermissionResponse> Handle(IReceiveContext<CheckRenamePermissionCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.CheckRenamePermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}