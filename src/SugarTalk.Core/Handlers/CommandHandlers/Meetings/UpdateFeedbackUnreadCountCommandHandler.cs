using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class UpdateFeedbackUnreadCountCommandHandler : ICommandHandler<UpdateFeedbackUnreadCountCommand, UpdateFeedbackUnreadCountResponse>
{
    private readonly IMeetingService _meetingService;
    
    public UpdateFeedbackUnreadCountCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<UpdateFeedbackUnreadCountResponse> Handle(IReceiveContext<UpdateFeedbackUnreadCountCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.UpdateFeedbackUnreadCountAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}