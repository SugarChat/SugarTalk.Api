using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings.Speak;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Speech;

public class GenerateChatRecordCommandHandler : ICommandHandler<GenerateChatRecordCommand, GenerateChatRecordResponse>
{
    private readonly IMeetingService _meetingService;
    
    public GenerateChatRecordCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GenerateChatRecordResponse> Handle(IReceiveContext<GenerateChatRecordCommand> context, CancellationToken cancellationToken)
    {
       return await _meetingService.GenerateChatRecordAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}