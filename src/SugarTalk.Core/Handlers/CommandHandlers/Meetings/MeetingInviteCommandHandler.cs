using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class MeetingInviteCommandHandler : ICommandHandler<MeetingInviteCommand, MeetingInviteResponse>
{
    private readonly IMeetingService _meetingService;

    public MeetingInviteCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<MeetingInviteResponse> Handle(IReceiveContext<MeetingInviteCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.MeetingInviteAsync(context.Message, cancellationToken).ConfigureAwait(false);
        
        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new MeetingInviteResponse
        {
            Token = @event.Token,
            HasMeetingPassword = @event.HasMeetingPassword
        };
    }
}