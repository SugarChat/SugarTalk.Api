using Newtonsoft.Json;

namespace SugarTalk.Api.Authentication.Wechat
{
    public class WechatPayload
    {
        [JsonProperty("openid")]
        public string OpenId { get; set; }
        
        [JsonProperty("nickname")]
        public string NickName { get; set; }
        
        [JsonProperty("sex")]
        public string Sex { get; set; }
        
        [JsonProperty("language")]
        public string Language { get; set; }
        
        [JsonProperty("city")]
        public string City { get; set; }
        
        [JsonProperty("province")]
        public string Province { get; set; }
        
        [JsonProperty("country")]
        public string Country { get; set; }
        
        [JsonProperty("headimgurl")]
        public string HeadImgUrl { get; set; }
        
        [JsonProperty("privilege")]
        public string[] Privilege { get; set; }
        
        [JsonProperty("unionid")]
        public string UnionId { get; set; }
    }
}