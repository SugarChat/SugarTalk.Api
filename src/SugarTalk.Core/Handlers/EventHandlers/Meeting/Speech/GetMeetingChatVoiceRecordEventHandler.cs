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
    private readonly ISugarTalkBackgroundJobClient _backgroundJobClient;

    public GetMeetingChatVoiceRecordEventHandler(ISugarTalkBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task Handle(IReceiveContext<GetMeetingChatVoiceRecordEvent> context, CancellationToken cancellationToken)
    {
        var shouldGenerateVoiceRecords = context.Message.ShouldGenerateVoiceRecords;
        
        if (shouldGenerateVoiceRecords is not { Count: > 0 }) return Task.CompletedTask;
        
        Log.Information("GetMeetingChatVoiceRecordEventHandler: {@ShouldGenerateVoiceRecords}", shouldGenerateVoiceRecords);
        
        var parentJobId = _backgroundJobClient.Enqueue<IMeetingService>(x=>
            x.ProcessGenerateMeetingChatVoiceRecordAsync(shouldGenerateVoiceRecords.First(), cancellationToken));

        shouldGenerateVoiceRecords?.Skip(1).Aggregate(parentJobId, (current, shouldGenerateVoiceRecord) =>
            _backgroundJobClient.ContinueJobWith<IMeetingService>(current,
                x => x.ProcessGenerateMeetingChatVoiceRecordAsync(shouldGenerateVoiceRecord, cancellationToken)));

        return Task.CompletedTask;
    }
}