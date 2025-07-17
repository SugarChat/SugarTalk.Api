using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Tencent;

public class StopCloudRecordingCommand : ICommand
{
    public ulong? SdkAppId { get; set; }
    
    public string TaskId { get; set; }
}

public class StopCloudRecordingResponse : SugarTalkResponse<StopCloudRecordingResponseResult>
{
}

public class StopCloudRecordingResponseResult 
{
    public string TaskId { get; set; }
    
    public string RequestId { get; set; }
}