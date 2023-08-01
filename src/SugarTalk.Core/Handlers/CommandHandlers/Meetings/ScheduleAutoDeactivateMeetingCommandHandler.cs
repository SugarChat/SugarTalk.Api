using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class ScheduleAutoDeactivateMeetingCommandHandler : ICommandHandler<ScheduleAutoDeactivateMeetingCommand>
{
    private readonly IMeetingService _meetingService;

    public ScheduleAutoDeactivateMeetingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task Handle(IReceiveContext<ScheduleAutoDeactivateMeetingCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.ScheduleAutoDeactivateMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}