using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class SchedulingGetMeetingTranscriptionUrlStatusCommandHandler : ICommandHandler<SchedulingGetMeetingTranscriptionUrlStatusCommand>
{
    private readonly IMeetingService _meetingService;

    public SchedulingGetMeetingTranscriptionUrlStatusCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task Handle(IReceiveContext<SchedulingGetMeetingTranscriptionUrlStatusCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.UpdateMeetingFileTranscriptionStatusAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}