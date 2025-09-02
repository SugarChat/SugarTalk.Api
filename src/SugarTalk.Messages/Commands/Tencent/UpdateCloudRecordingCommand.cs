using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Tencent;

public class UpdateCloudRecordingCommand : ICommand
{
    public ulong? SdkAppId { get; set; }
    
    public string TaskId { get; set; }
    
    public MixLayoutParams MixLayoutParams { get; set; }
    
    public SubscribeStreamUserIds SubscribeStreamUserIds { get; set; }
}

public class MixLayoutParams
{
    public ulong? MixLayoutMode { get; set; }
    
    public MixLayout[] MixLayoutList { get; set; }
    
    public string BackGroundColor { get; set; }
    
    public string MaxResolutionUserId { get; set; }
    
    public ulong? MediaId { get; set; }
    
    public string BackgroundImageUrl { get; set; }
    
    public ulong? PlaceHolderMode { get; set; }
    
    public ulong? BackgroundImageRenderMode { get; set; }
    
    public string DefaultSubBackgroundImage { get; set; }
    
    public WaterMark[] WaterMarkList { get; set; }
    
    public ulong? RenderMode { get; set; }
    
    public ulong? MaxResolutionUserAlign { get; set; }
}

public class MixLayout
{
    public ulong? Top { get; set; }
    
    public ulong? Left { get; set; }
    
    public ulong? Width { get; set; }
    
    public ulong? Height { get; set; }
    
    public string UserId { get; set; }
    
    public ulong? Alpha { get; set; }
    
    public ulong? RenderMode { get; set; }
    
    public ulong? MediaId { get; set; }
    
    public ulong? ImageLayer { get; set; }
    
    public string SubBackgroundImage { get; set; }
}

public class WaterMark
{
    public ulong? WaterMarkType { get; set; }
    
    public WaterMarkImage WaterMarkImage { get; set; }
    
    public WaterMarkChar WaterMarkChar { get; set; }
    
    public WaterMarkTimestamp WaterMarkTimestamp { get; set; }
}

public class WaterMarkImage
{
    public string WaterMarkUrl { get; set; }

    public ulong? Top { get; set; }

    public ulong? Left { get; set; }

    public ulong? Width { get; set; }

    public ulong? Height { get; set; }
}

public class WaterMarkChar
{
    public ulong? Top { get; set; }
    
    public ulong? Left { get; set; }

    public ulong? Width { get; set; }
    
    public ulong? Height { get; set; }
    
    public string Chars { get; set; }
    
    public ulong? FontSize { get; set; }

    public string FontColor { get; set; }
    
    public string BackGroundColor { get; set; }
    
    public string Font { get; set; }
}

public class WaterMarkTimestamp
{
    public ulong? Pos { get; set; }
    
    public ulong? TimeZone { get; set; }
    
    public string Font { get; set; } 
}

public class UpdateCloudRecordingResponse : SugarTalkResponse<UpdateCloudRecordingResponseResult>
{
}

public class UpdateCloudRecordingResponseResult 
{
    public string TaskId { get; set; }
    
    public string RequestId { get; set; }
}