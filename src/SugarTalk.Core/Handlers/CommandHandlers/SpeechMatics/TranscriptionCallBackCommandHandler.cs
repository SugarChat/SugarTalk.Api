using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Core.Services.Jobs;
using SugarTalk.Core.Services.Smarties;
using SugarTalk.Messages.Commands.SpeechMatics;

namespace SugarTalk.Core.Handlers.CommandHandlers.SpeechMatics;

public class TranscriptionCallBackCommandHandler  : ICommandHandler<TranscriptionCallBackCommand>
{
    private readonly ISugarTalkBackgroundJobClient _backgroundJobClient;
    
    public TranscriptionCallBackCommandHandler(ISugarTalkBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }
    
    public async Task Handle(IReceiveContext<TranscriptionCallBackCommand> context, CancellationToken cancellationToken)
    {
        _backgroundJobClient.Enqueue<ISmartiesService>(x => x.HandleTranscriptionCallbackAsync(context.Message, cancellationToken));
    }
}