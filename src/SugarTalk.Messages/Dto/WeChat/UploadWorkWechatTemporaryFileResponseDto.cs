using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.WeChat;

public class UploadWorkWechatTemporaryFileResponseDto : WorkWeChatResponseDto
{
    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("media_id")]
    public string MediaId { get; set; }
    
    [JsonProperty("created_at")]
    public string CreateAt { get; set; }
}