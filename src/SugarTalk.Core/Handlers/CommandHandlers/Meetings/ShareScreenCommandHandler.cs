using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class ShareScreenCommandHandler : ICommandHandler<ShareScreenCommand, ShareScreenResponse>
{
    private readonly IMeetingService _meetingService;

    public ShareScreenCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<ShareScreenResponse> Handle(IReceiveContext<ShareScreenCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.ShareScreenAsync(context.Message, cancellationToken).ConfigureAwait(false);

        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new ShareScreenResponse
        {
            Data = new ShareScreenResponseData
            {
                Response = @event.Response,
                MeetingUserSession = @event.MeetingUserSession
            }
        };
    }
}