using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class UpdateMeetingProblemFeedbackUnreadCountCommandHandler : ICommandHandler<UpdateMeetingProblemFeedbackUnreadCountCommand, UpdateMeetingProblemFeedbackUnreadCountResponse>
{
    private readonly IMeetingService _meetingService;
    
    public UpdateMeetingProblemFeedbackUnreadCountCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<UpdateMeetingProblemFeedbackUnreadCountResponse> Handle(IReceiveContext<UpdateMeetingProblemFeedbackUnreadCountCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.UpdateMeetingProblemFeedbackUnreadCountAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}