using SugarTalk.Messages.Commands.Tencent;
using SugarTalk.Messages.Enums.Tencent;

namespace SugarTalk.Messages.Dto.SugarTalk;

public class CloudRecordingCallBackDto
{
    public CloudRecordingEventGroupType EventGroupId { get; set; }
    
    public CloudRecordingEventType EventType { get; set; }
    
    public ulong CallbackTs { get; set; }
    
    public EventInfo EventInfo { get; set; }
}