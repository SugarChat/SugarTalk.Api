using Newtonsoft.Json;

namespace SugarTalk.Api.Middlewares.Authentication
{
    public class FacebookPayload
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("email")]
        public string Email { get; set; }
        
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("picture")]
        public FacebookPicture Picture { get; set; }
    }

    public class FacebookPicture
    {
        [JsonProperty("data")]
        public FacebookPictureData Data { get; set; }
    }

    public class FacebookPictureData
    {
        [JsonProperty("height")]
        public int Height { get; set; }
        
        [JsonProperty("is_silhouette")]
        public bool IsSilhouette { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("width")]
        public int Width { get; set; }
    }
}