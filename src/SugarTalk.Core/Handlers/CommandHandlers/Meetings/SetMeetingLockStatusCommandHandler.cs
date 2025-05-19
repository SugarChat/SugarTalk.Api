using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class SetMeetingLockStatusCommandHandler  : ICommandHandler<SetMeetingLockStatusCommand, SetMeetingLockStatusResponse>
{
    private readonly IMeetingService _meetingService;

    public SetMeetingLockStatusCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<SetMeetingLockStatusResponse> Handle(IReceiveContext<SetMeetingLockStatusCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.SetMeetingLockStatusResponseAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}