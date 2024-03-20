using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Serilog;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Events.Meeting.Speech;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting.Speech;

public class GetMeetingChatVoiceRecordEventHandler : IEventHandler<GetMeetingChatVoiceRecordEvent>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingChatVoiceRecordEventHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public Task Handle(IReceiveContext<GetMeetingChatVoiceRecordEvent> context, CancellationToken cancellationToken)
    {
        if (context.Message.ShouldGenerateVoiceRecords.Count <= 0) return Task.CompletedTask;
        
        Log.Information("GetMeetingChatVoiceRecordEventHandler: {ShouldGenerateVoiceRecordsCount}", context.Message.ShouldGenerateVoiceRecords.Count);
        
        foreach (var shouldGenerateVoiceRecord in context.Message.ShouldGenerateVoiceRecords)
        { 
            _meetingService.ShouldGenerateMeetingChatVoiceRecordAsync(shouldGenerateVoiceRecord, cancellationToken);
        }

        return Task.CompletedTask;
    }
}