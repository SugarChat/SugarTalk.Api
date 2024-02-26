using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class CheckAppointmentMeetingDateCommandHandler : ICommandHandler<CheckAppointmentMeetingDateCommand>
{
    private readonly IMeetingProcessJobService _meetingProcessJobService;

    public CheckAppointmentMeetingDateCommandHandler(IMeetingProcessJobService meetingProcessJobService)
    {
        _meetingProcessJobService = meetingProcessJobService;
    }

    public async Task Handle(IReceiveContext<CheckAppointmentMeetingDateCommand> context, CancellationToken cancellationToken)
    {
        await _meetingProcessJobService.CheckAppointmentMeetingDateAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}