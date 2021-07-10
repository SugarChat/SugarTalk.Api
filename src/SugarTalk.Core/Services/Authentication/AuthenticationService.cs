using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SugarTalk.Core.Settings;
using SugarTalk.Messages.Dtos.Authentication;
using SugarTalk.Messages.Requests.Authentication;

namespace SugarTalk.Core.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task<GetGoogleAccessTokenResponse> GetGoogleAccessToken(GetGoogleAccessTokenRequest request,
            CancellationToken cancellationToken);

        Task<GetFacebookAccessTokenResponse> GetFacebookAccessToken(GetFacebookAccessTokenRequest request,
            CancellationToken cancellationToken);
    }
    
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<GoogleSettings> _googleSettings;
        private readonly IOptions<FacebookSettings> _facebookSettings;

        public AuthenticationService(IHttpClientFactory httpClientFactory, IOptions<GoogleSettings> googleSettings, IOptions<FacebookSettings> facebookSettings)
        {
            _googleSettings = googleSettings;
            _facebookSettings = facebookSettings;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetGoogleAccessTokenResponse> GetGoogleAccessToken(GetGoogleAccessTokenRequest request,
            CancellationToken cancellationToken)
        {
            var requestUrl =
                $"https://oauth2.googleapis.com/token?code={request.Code}&client_id={_googleSettings.Value.ClientId}&client_secret={_googleSettings.Value.ClientSecret}&grant_type=authorization_code&redirect_uri={request.RedirectUri}";

            var response = await _httpClientFactory.CreateClient("google")
                .PostAsync(requestUrl, null, cancellationToken).ConfigureAwait(false);

            var content = await response.Content.ReadAsStringAsync();
            
            var accessToken = JsonConvert.DeserializeObject<GoogleAccessTokenDto>(content);

            return new GetGoogleAccessTokenResponse()
            {
                AccessToken = accessToken
            };
        }

        public async Task<GetFacebookAccessTokenResponse> GetFacebookAccessToken(GetFacebookAccessTokenRequest request,
            CancellationToken cancellationToken)
        {
            var requestUrl =
                $"https://graph.facebook.com/oauth/access_token?code={request.Code}&client_id={_facebookSettings.Value.ClientId}&client_secret={_facebookSettings.Value.ClientSecret}";
            
            var response = await _httpClientFactory.CreateClient("facebook").GetStringAsync(requestUrl);

            var accessToken = JsonConvert.DeserializeObject<FacebookAccessTokenDto>(response);

            return new GetFacebookAccessTokenResponse()
            {
                AccessToken = accessToken
            };
        }
    }
}