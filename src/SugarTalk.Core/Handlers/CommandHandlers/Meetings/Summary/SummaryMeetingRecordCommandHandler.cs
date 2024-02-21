using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings.Summary;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Summary;

public class SummaryMeetingRecordCommandHandler : ICommandHandler<SummaryMeetingRecordCommand, SummaryMeetingRecordResponse>
{
    private readonly IMeetingService _meetingService;

    public SummaryMeetingRecordCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<SummaryMeetingRecordResponse> Handle(IReceiveContext<SummaryMeetingRecordCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.SummaryMeetingRecordAsync(context.Message, cancellationToken).ConfigureAwait(false);

        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new SummaryMeetingRecordResponse { Data = @event.Summary };
    }
}