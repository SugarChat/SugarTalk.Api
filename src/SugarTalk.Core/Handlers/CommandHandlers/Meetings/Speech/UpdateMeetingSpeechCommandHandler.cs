using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Speech;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Speech;

public class UpdateMeetingSpeechCommandHandler : ICommandHandler<UpdateMeetingSpeechCommand, UpdateMeetingAudioResponse>
{
    private readonly IMeetingService _meetingService;

    public UpdateMeetingSpeechCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<UpdateMeetingAudioResponse> Handle(
        IReceiveContext<UpdateMeetingSpeechCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.UpdateMeetingSpeechAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}