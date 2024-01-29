using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Speech;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Speech;

public class UpdateMeetingSpeechCommandHandler : ICommandHandler<UpdateMeetingSpeechCommand, UpdateMeetingSpeechResponse>
{
    private readonly IMeetingService _meetingService;

    public UpdateMeetingSpeechCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<UpdateMeetingSpeechResponse> Handle(
        IReceiveContext<UpdateMeetingSpeechCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.UpdateMeetingSpeechAsync(context.Message, cancellationToken).ConfigureAwait(false);
        
        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new UpdateMeetingSpeechResponse
        {
            Data = @event.Result
        };
    }
}