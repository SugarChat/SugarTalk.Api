using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Speech;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Speech;

public class SaveMeetingAudioCommandHandler : ICommandHandler<SaveMeetingAudioCommand, SaveMeetingAudioResponse>
{
    private readonly IMeetingService _meetingService;

    public SaveMeetingAudioCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<SaveMeetingAudioResponse> Handle(IReceiveContext<SaveMeetingAudioCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.SaveMeetingAudioAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}