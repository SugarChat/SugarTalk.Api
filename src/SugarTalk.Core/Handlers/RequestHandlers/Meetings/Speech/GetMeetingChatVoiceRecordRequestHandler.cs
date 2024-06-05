using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Speech;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings.Speech;

public class GetMeetingChatVoiceRecordRequestHandler : IRequestHandler<GetMeetingChatVoiceRecordRequest, GetMeetingChatVoiceRecordResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingChatVoiceRecordRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<GetMeetingChatVoiceRecordResponse> Handle(IReceiveContext<GetMeetingChatVoiceRecordRequest> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.GetMeetingChatVoiceRecordAsync(context.Message, cancellationToken).ConfigureAwait(false);
        
        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new GetMeetingChatVoiceRecordResponse
        {
            Data = @event.MeetingSpeech
        };
    }
}