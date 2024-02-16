using System.ComponentModel;

namespace SugarTalk.Messages.Enums.LiveKit;

public enum EgressEncodingOptionsPreset
{
    [Description("H264_720P_30")]
    H264720P30 = 0,
    
    [Description("H264_720P_60")]
    H264720P60 = 1,
    
    [Description("H264_1080P_30")]
    H2641080P30 = 2,
    
    [Description("H264_1080P_60")]
    H2641080P60 = 3,
    
    [Description("PORTRAIT_H264_720P_30")]
    PortraitH264720P30 = 4,
    
    [Description("PORTRAIT_H264_720P_60")]
    PortraitH264720P60 = 5,
    
    [Description("PORTRAIT_H264_1080P_30")]
    PortraitH2641080P30 = 6,
    
    [Description("PORTRAIT_H264_1080P_60")]
    PortraitH2641080P60 = 7
}