using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class EndMeetingCommandHandler : ICommandHandler<EndMeetingCommand, EndMeetingResponse>
{
    private readonly IMeetingService _meetingService;

    public EndMeetingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<EndMeetingResponse> Handle(IReceiveContext<EndMeetingCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.EndMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);
        
        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new EndMeetingResponse { Data = @event.Data };
    }
}