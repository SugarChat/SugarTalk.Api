using Newtonsoft.Json;

namespace SugarTalk.Messages.Dtos.Authentication
{
    public class GoogleAccessTokenDto
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        
        [JsonProperty("scope")]
        public string Scope { get; set; }
        
        [JsonProperty("id_token")]
        public string IdToken { get; set; }
    }
}