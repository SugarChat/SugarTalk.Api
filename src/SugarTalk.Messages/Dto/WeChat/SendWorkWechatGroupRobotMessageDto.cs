using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.WeChat;

public class SendWorkWechatGroupRobotMessageDto
{
    [JsonProperty("msgtype")]
    public string MsgType { get; set; }
    
    [JsonProperty("text")]
    public SendWorkWechatGroupRobotTextDto Text { get; set; }
    
    [JsonProperty("file")]
    public SendWorkWechatGroupRobotFileDto File { get; set; }
    
    [JsonProperty("voice")]
    public SendWorkWechatGroupRobotFileDto Voice { get; set; }
}

public class SendWorkWechatGroupRobotTextDto
{
    [JsonProperty("content")]
    public string Content { get; set; }
    
    [JsonProperty("mentioned_list", NullValueHandling = NullValueHandling.Ignore)]
    public string[] MentionedList { get; set; }
    
    [JsonProperty("mentioned_mobile_list", NullValueHandling = NullValueHandling.Ignore)]
    public string MentionedMobileList { get; set; }
}

public class SendWorkWechatGroupRobotFileDto
{
    [JsonProperty("media_id")]
    public string MediaId { get; set; }
}

public class WorkWeChatResponseDto
{
    [JsonProperty("errcode", NullValueHandling = NullValueHandling.Ignore)]
    public int ErrorCode { get; set; }
    
    [JsonProperty("errmsg", NullValueHandling = NullValueHandling.Ignore)]
    public string ErrorMsg { get; set; }
}

public class UploadWorkWechatGroupRobotFileResponseDto : WorkWeChatResponseDto
{
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public string Type { get; set; }
    
    [JsonProperty("media_id", NullValueHandling = NullValueHandling.Ignore)]
    public string MediaId { get; set; }
    
    [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
    public string CreatedAt { get; set; }
}