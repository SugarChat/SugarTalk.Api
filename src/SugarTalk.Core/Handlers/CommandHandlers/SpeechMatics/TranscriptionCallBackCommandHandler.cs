using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Messages.Commands.SpeechMatics;

namespace SugarTalk.Core.Handlers.CommandHandlers.SpeechMatics;

public class TranscriptionCallBackCommandHandler  : ICommandHandler<TranscriptionCallBackCommand>
{
    public TranscriptionCallBackCommandHandler()
    {
        
    }
    
    public Task Handle(IReceiveContext<TranscriptionCallBackCommand> context, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}