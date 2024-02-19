using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class DeleteMeetingHistoryCommandHandler : ICommandHandler<DeleteMeetingHistoryCommand>
{
    private readonly IMeetingService _meetingService;

    public DeleteMeetingHistoryCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task Handle(IReceiveContext<DeleteMeetingHistoryCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.DeleteMeetingHistoryAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}