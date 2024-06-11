using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class UpdateMeetingRecordUrlCommandHandler : ICommandHandler<UpdateMeetingRecordUrlCommand>
{
    private readonly IMeetingService _meetingService;

    public UpdateMeetingRecordUrlCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task Handle(IReceiveContext<UpdateMeetingRecordUrlCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.UpdateMeetingRecordUrlAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}