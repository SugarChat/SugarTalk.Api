using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class OutMeetingCommandHandler : ICommandHandler<OutMeetingCommand, OutMeetingResponse>
{
    private readonly IMeetingService _meetingService;

    public OutMeetingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<OutMeetingResponse> Handle(IReceiveContext<OutMeetingCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.OutMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);   
        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new OutMeetingResponse();
    }
}