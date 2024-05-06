using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class GeneralRestartRecordCommandHandler : ICommandHandler<GeneralRestartRecordCommand>
{
    private readonly IMeetingService _meetingService;

    public GeneralRestartRecordCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task Handle(IReceiveContext<GeneralRestartRecordCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.GeneralRestartRecordAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}