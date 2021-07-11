using Newtonsoft.Json;

namespace SugarTalk.Messages.Dtos.Authentication
{
    public class FacebookAccessTokenDto
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }
    }
}