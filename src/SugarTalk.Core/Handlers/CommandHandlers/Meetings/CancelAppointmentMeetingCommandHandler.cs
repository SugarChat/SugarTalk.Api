using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class CancelAppointmentMeetingCommandHandler : ICommandHandler<CancelAppointmentMeetingCommand>
{
    private readonly IMeetingService _meetingService;

    public CancelAppointmentMeetingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task Handle(IReceiveContext<CancelAppointmentMeetingCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.CancelAppointmentMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}