using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Serilog;
using SugarTalk.Core.Services.Jobs;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Events.Meeting.Speech;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting.Speech;

public class GetMeetingChatVoiceRecordEventHandler : IEventHandler<GetMeetingChatVoiceRecordEvent>
{
    private readonly IMeetingService _meetingService;
    private readonly ISugarTalkBackgroundJobClient _backgroundJobClient;

    public GetMeetingChatVoiceRecordEventHandler(IMeetingService meetingService, ISugarTalkBackgroundJobClient backgroundJobClient)
    {
        _meetingService = meetingService;
        _backgroundJobClient = backgroundJobClient;
    }

    public Task Handle(IReceiveContext<GetMeetingChatVoiceRecordEvent> context, CancellationToken cancellationToken)
    {
        if (context.Message.ShouldGenerateVoiceRecords.Count <= 0) return Task.CompletedTask;;
        
        Log.Information("GetMeetingChatVoiceRecordEventHandler: {ShouldGenerateVoiceRecordsCount}", context.Message.ShouldGenerateVoiceRecords.Count);
        
        var parentJobId = _backgroundJobClient.Enqueue(() => _meetingService.ShouldGenerateMeetingChatVoiceRecordAsync(context.Message.ShouldGenerateVoiceRecords.First(), cancellationToken));
        
        context.Message.ShouldGenerateVoiceRecords.Skip(1).Aggregate(parentJobId, (current, shouldGenerateVoiceRecord)=>
            _backgroundJobClient.ContinueJobWith(current, () => _meetingService.ShouldGenerateMeetingChatVoiceRecordAsync(shouldGenerateVoiceRecord, cancellationToken)));
        
        return Task.CompletedTask;
    }
}