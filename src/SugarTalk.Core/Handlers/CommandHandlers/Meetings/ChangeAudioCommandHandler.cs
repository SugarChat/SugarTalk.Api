using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class ChangeAudioCommandHandler : ICommandHandler<ChangeAudioCommand, ChangeAudioResponse>
{
    private readonly IMeetingService _meetingService;

    public ChangeAudioCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<ChangeAudioResponse> Handle(IReceiveContext<ChangeAudioCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.ChangeAudioAsync(context.Message, cancellationToken).ConfigureAwait(false);

        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new ChangeAudioResponse
        {
            Data = new ChangeAudioResponseData
            {
                MeetingUserSession = @event.MeetingUserSession
            }
        };
    }
}