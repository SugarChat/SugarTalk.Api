using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class StartMeetingRecordingCommandHandler : ICommandHandler<StartMeetingRecordingCommand, StartMeetingRecordingResponse>
{
    private readonly IMeetingService _meetingService;

    public StartMeetingRecordingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<StartMeetingRecordingResponse> Handle(IReceiveContext<StartMeetingRecordingCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.StartMeetingRecordingAsync(context.Message, cancellationToken).ConfigureAwait(false);

        return new StartMeetingRecordingResponse
        {
            MeetingRecordId = @event.MeetingRecordId,
            EgressId = @event.EgressId
        };
    }
}