using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class CancelAppointmentMeetingCommandHandler : ICommandHandler<CancelAppointmentMeetingCommand, CancelAppointmentMeetingResponse>
{
    private readonly IMeetingService _meetingService;

    public CancelAppointmentMeetingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<CancelAppointmentMeetingResponse> Handle(IReceiveContext<CancelAppointmentMeetingCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.CancelAppointmentMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);

        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new CancelAppointmentMeetingResponse();
    }
}