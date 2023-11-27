using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Speech;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Speech;

public class SaveMeetingAudioCommandHandler : ICommandHandler<SaveMeetingAudioCommand>
{
    private readonly IMeetingService _meetingService;

    public SaveMeetingAudioCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task Handle(IReceiveContext<SaveMeetingAudioCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.SaveMeetingAudioAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}