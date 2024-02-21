using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class DeleteMeetingRecordCommandHandler : ICommandHandler<DeleteMeetingRecordCommand>
{
    private readonly IMeetingService _meetingService;

    public DeleteMeetingRecordCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task Handle(IReceiveContext<DeleteMeetingRecordCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.DeleteMeetingRecordAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}