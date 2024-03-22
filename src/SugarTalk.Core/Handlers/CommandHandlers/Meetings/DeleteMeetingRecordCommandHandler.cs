using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class DeleteMeetingRecordCommandHandler : ICommandHandler<DeleteMeetingRecordCommand, DeleteMeetingRecordResponse>
{
    private readonly IMeetingService _meetingService;

    public DeleteMeetingRecordCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<DeleteMeetingRecordResponse> Handle(IReceiveContext<DeleteMeetingRecordCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.DeleteMeetingRecordAsync(context.Message, cancellationToken).ConfigureAwait(false);
        
        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new DeleteMeetingRecordResponse();
    }
}