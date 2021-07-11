using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SugarTalk.Core;
using SugarTalk.Core.Services.Authentication;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Api.Middlewares.Authentication
{
    public class FacebookAuthenticationHandler : AuthenticationHandler<FacebookAuthenticationOptions>
    {
        private readonly ITokenService _tokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        
        public FacebookAuthenticationHandler(IOptionsMonitor<FacebookAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IHttpClientFactory httpClientFactory, ITokenService tokenService) : base(options, logger, encoder, clock)
        {
            _tokenService = tokenService;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.NoResult();
            
            var auth = Request.Headers["Authorization"].ToString();
            
            if (string.IsNullOrWhiteSpace(auth) || !auth.Contains("Bearer"))
                return AuthenticateResult.NoResult();
            
            var authHeaderValue = auth.Split(' ');
            
            if (authHeaderValue.Length < 2)
                return AuthenticateResult.NoResult();
            
            var bearerToken = authHeaderValue[1];

            var facebookUserInfoUrl =
                $"https://graph.facebook.com/me?access_token={bearerToken}&fields=id,name,email,picture";

            var payload = await _httpClientFactory.CreateClient()
                .GetFromJsonAsync<FacebookPayload>(facebookUserInfoUrl)
                .ConfigureAwait(false);
            
            if (payload == null) return AuthenticateResult.NoResult();
            
            return AuthenticateResult.Success(new AuthenticationTicket
            (
                new ClaimsPrincipal(new ClaimsIdentity(GetClaims(payload), "Facebook")
            ), new AuthenticationProperties {IsPersistent = false}, Scheme.Name));
        }
        
        private IEnumerable<Claim> GetClaims(FacebookPayload payload)
        {
            var name = payload.Name;
            var email = payload.Email ?? "";
            var picture = payload.Picture?.Data?.Url ?? "";
            var thirdPartyId = payload.Id;
            
            return new List<Claim>
            {
                new(ClaimTypes.Name, name),
                new(ClaimTypes.Email, email),
                new(SugarTalkClaimType.Picture, picture),
                new(SugarTalkClaimType.ThirdPartyId, thirdPartyId),
                new(SugarTalkClaimType.ThirdPartyFrom, ThirdPartyFrom.Facebook.ToString())
            };
        }
    }
}