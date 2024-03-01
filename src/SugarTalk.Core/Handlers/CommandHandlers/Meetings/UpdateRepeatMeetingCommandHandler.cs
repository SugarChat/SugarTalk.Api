using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class UpdateRepeatMeetingCommandHandler : ICommandHandler<UpdateRepeatMeetingCommand>
{
    private readonly IMeetingProcessJobService _meetingProcessJobService;

    public UpdateRepeatMeetingCommandHandler(IMeetingProcessJobService meetingProcessJobService)
    {
        _meetingProcessJobService = meetingProcessJobService;
    }

    public async Task Handle(IReceiveContext<UpdateRepeatMeetingCommand> context, CancellationToken cancellationToken)
    {
        await _meetingProcessJobService.UpdateRepeatMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}