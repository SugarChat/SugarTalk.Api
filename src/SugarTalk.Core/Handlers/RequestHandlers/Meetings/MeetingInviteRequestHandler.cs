using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class MeetingInviteRequestHandler : IRequestHandler<MeetingInviteRequest, MeetingInviteResponse>
{
    private readonly IMeetingService _meetingService;

    public MeetingInviteRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<MeetingInviteResponse> Handle(IReceiveContext<MeetingInviteRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.MeetingInviteAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}