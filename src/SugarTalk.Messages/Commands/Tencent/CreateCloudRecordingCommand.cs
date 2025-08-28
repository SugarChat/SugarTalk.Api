using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Tencent;

public class CreateCloudRecordingCommand : ICommand
{
    public ulong? SdkAppId { get; set; }

    public string RoomId { get; set; }
    
    public RecordParams RecordParams { get; set; }
    
    public ulong? RoomIdType { get; set; }
    
    public ulong? ResourceExpiredHour { get; set; }
    
    public string PrivateMapKey { get; set; }
    
    public MixTranscodeParams MixTranscodeParams { get; set; }
    
    public MixLayoutParams MixLayoutParams { get; set; }
}

public class RecordParams
{
    public ulong? RecordMode { get; set; }
    
    public ulong? StreamType { get; set; }

    public ulong? MaxIdleTime { get; set; }
    
    public SubscribeStreamUserIds SubscribeStreamUserIds { get; set; }
    
    public ulong? OutputFormat { get; set; }
    
    public ulong? AvMerge { get; set; }
    
    public ulong? MaxMediaFileDuration { get; set; }
    
    public ulong? FillType { get; set; }
}

public class SubscribeStreamUserIds
{
    public string[] SubscribeAudioUserIds { get; set; }
    
    public string[] UnSubscribeAudioUserIds { get; set; }
    
    public string[] SubscribeVideoUserIds { get; set; }
    
    public string[] UnSubscribeVideoUserIds { get; set; }
}

public class MixTranscodeParams
{
    public VideoParams VideoParams { get; set; }
    
    public AudioParams AudioParams { get; set; }
}

public class AudioParams
{
    public ulong? SampleRate { get; set; }
    
    public ulong? Channel { get; set; }
    
    public ulong? BitRate { get; set; }
}

public class VideoParams
{
    public ulong? Width { get; set; }
    
    public ulong? Height { get; set; }
    
    public ulong? Fps { get; set; }
    
    public ulong? BitRate { get; set; }
    
    public ulong? Gop { get; set; }
}

public class StartCloudRecordingResponse : SugarTalkResponse<CreateCloudRecordingResponseResult>
{
}

public class CreateCloudRecordingResponseResult 
{
    public string TaskId { get; set; }
    
    public string RequestId { get; set; }
    
    public Guid MeetingRecordId { get; set; }
}