using Mediator.Net.Contracts;
using Newtonsoft.Json.Linq;
using SugarTalk.Messages.Enums.Tencent;

namespace SugarTalk.Messages.Commands.Tencent;

public class CloudRecordingCallBackCommand : ICommand
{
    public CloudRecordingEventGroupType EventGroupId { get; set; }
    
    public CloudRecordingEventType EventType { get; set; }
    
    public ulong CallbackTs { get; set; }
    
    public EventInfo EventInfo { get; set; }
}

public class EventInfo
{
    public string RoomId { get; set; }
    
    public long EventTs { get; set; }
    
    public long EventMsTs { get; set; }
    
    public string UserId { get; set; }
    
    public string TaskId { get; set; }
    
    public JObject Payload { get; set; }
}
