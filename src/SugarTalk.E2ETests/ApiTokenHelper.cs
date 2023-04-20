using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace SugarTalk.E2ETests;

public class ApiTokenHelper
{
    public static async Task<string> GetWiltechsUserToken()
    {
        using (var wiltechsClient = new HttpClient())
        {
            wiltechsClient.BaseAddress = new Uri("http://passtest.wiltechs.com/");

            wiltechsClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", "NDUwYzZjMDNmYzQ0YzQzYjo3OWQ5MDJkYmZlM2Q3ODFm");

            var nvc = new List<KeyValuePair<string, string>>
            {
                new ("grant_type", "password"),
                new ("username", "bruce.l"),
                new ("password", "000000")
            };
            
            var response = await wiltechsClient.PostAsync("token", new FormUrlEncodedContent(nvc)).ConfigureAwait(false);
            
            response.EnsureSuccessStatusCode();

            var stringResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            var wiltechsTokenInfo = JsonConvert.DeserializeObject<WiltechsTokenInfo>(stringResponse);

            return wiltechsTokenInfo?.AccessToken;
        }
    }
}

internal class WiltechsTokenInfo
{
    [JsonProperty("access_token")] 
    public string AccessToken { get; set; }

    [JsonProperty("userName")] 
    public string UserName { get; set; }

    [JsonProperty("expires_in")] 
    public int ExpiresIn { get; set; }
}