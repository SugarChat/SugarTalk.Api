using System.Threading;
using Mediator.Net.Context;
using System.Threading.Tasks;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings.Speak;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Speak;

public class RecordMeetingSpeakCommandHandler : ICommandHandler<RecordMeetingSpeakCommand, RecordMeetingSpeakResponse>
{
    private readonly IMeetingService _meetingService;

    public RecordMeetingSpeakCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<RecordMeetingSpeakResponse> Handle(IReceiveContext<RecordMeetingSpeakCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.RecordMeetingSpeakAsync(context.Message, cancellationToken).ConfigureAwait(false);

        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new RecordMeetingSpeakResponse
        {
            Data = @event.MeetingSpeakDetail
        };
    }
}