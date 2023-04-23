using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Serilog;
using SugarTalk.Core.Services.Authentication;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Api.Authentication.Facebook
{
    public class FacebookAuthenticationHandler : AuthenticationHandlerBase<FacebookAuthenticationOptions>
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
            if (Request.HttpContext.User.Identity == null || Request.HttpContext.User.Identity.IsAuthenticated)
                return AuthenticateResult.NoResult();
            
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.NoResult();
            
            var auth = Request.Headers["Authorization"].ToString();
            
            if (string.IsNullOrWhiteSpace(auth) || !auth.Contains("Bearer"))
                return AuthenticateResult.NoResult();
            
            var authHeaderValue = auth.Split(' ');
            
            if (authHeaderValue.Length < 2)
                return AuthenticateResult.NoResult();
            
            var bearerToken = authHeaderValue[1];

            var payload = await _tokenService
                .GetPayloadFromMemoryOrDb<FacebookPayload>(bearerToken, ThirdPartyFrom.Facebook)
                .ConfigureAwait(false);

            if (payload == null)
            {
                var facebookUserInfoUrl =
                    $"https://graph.facebook.com/me?access_token={bearerToken}&fields=id,name,email,picture";

                try
                {
                    payload = await _httpClientFactory.CreateClient()
                        .GetFromJsonAsync<FacebookPayload>(facebookUserInfoUrl)
                        .ConfigureAwait(false);

                    await _tokenService.PersistPayloadToMemoryAndDb(bearerToken, ThirdPartyFrom.Facebook, payload)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Facebook authentication failed: {Exception}", ex.Message);
                    
                    return null;
                }
            }
            
            if (payload == null) return AuthenticateResult.NoResult();
            
            var principal =
                new ClaimsPrincipal(new ClaimsIdentity(GetClaims(payload), ThirdPartyFrom.Facebook.ToString()));

            return AuthenticateResult.Success(new AuthenticationTicket(principal,
                new AuthenticationProperties {IsPersistent = false}, Scheme.Name));
        }
    }
}