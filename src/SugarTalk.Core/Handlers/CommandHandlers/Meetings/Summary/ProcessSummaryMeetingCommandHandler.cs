using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings.Summary;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Summary;

public class ProcessSummaryMeetingCommandHandler : ICommandHandler<ProcessSummaryMeetingCommand>
{
    private readonly IMeetingService _meetingService;

    public ProcessSummaryMeetingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task Handle(IReceiveContext<ProcessSummaryMeetingCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.SummarizeMeetingInTargetLanguageAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}