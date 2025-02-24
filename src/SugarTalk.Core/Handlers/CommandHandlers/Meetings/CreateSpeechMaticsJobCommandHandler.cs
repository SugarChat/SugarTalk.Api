using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class CreateSpeechMaticsJobCommandHandler : ICommandHandler<CreateSpeechMaticsJobCommand>
{
    private readonly IMeetingService _meetingService;

    public CreateSpeechMaticsJobCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task Handle(IReceiveContext<CreateSpeechMaticsJobCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.RetrySpeechMaticsJobAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}