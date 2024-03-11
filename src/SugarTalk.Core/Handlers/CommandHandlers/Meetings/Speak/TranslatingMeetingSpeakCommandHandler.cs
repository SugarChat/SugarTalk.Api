using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Speak;

public class TranslatingMeetingSpeakCommandHandler : ICommandHandler<TranslatingMeetingSpeakCommand, TranslatingMeetingSpeakResponse>
{
    private readonly IMeetingService _meetingService;

    public TranslatingMeetingSpeakCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<TranslatingMeetingSpeakResponse> Handle(IReceiveContext<TranslatingMeetingSpeakCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.TranslatingMeetingSpeakAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}