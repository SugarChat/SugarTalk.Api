using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class AddMeetingProblemFeedbackCommandHandler : ICommandHandler<AddMeetingProblemFeedbackCommand, AddMeetingProblemFeedbackResponse>
{
    private readonly IMeetingService _meetingService;

    public AddMeetingProblemFeedbackCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<AddMeetingProblemFeedbackResponse> Handle(IReceiveContext<AddMeetingProblemFeedbackCommand> context, CancellationToken cancellationToken)
    { 
        return await _meetingService.AddMeetingProblemFeedbackAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}