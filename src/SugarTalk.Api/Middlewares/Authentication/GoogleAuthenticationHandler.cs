using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using SugarTalk.Core;
using SugarTalk.Core.Services.Authentication;
using SugarTalk.Messages.Enums;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace SugarTalk.Api.Middlewares.Authentication
{
    public class GoogleAuthenticationHandler : AuthenticationHandlerBase<GoogleAuthenticationOptions>
    {
        private readonly ITokenService _tokenService;
        
        public GoogleAuthenticationHandler(IOptionsMonitor<GoogleAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, ITokenService tokenService) : base(options, logger, encoder, clock)
        {
            _tokenService = tokenService;
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

            var payload = await _tokenService.GetPayloadFromMemoryOrDb<Payload>(bearerToken, ThirdPartyFrom.Google)
                .ConfigureAwait(false);

            if (payload == null)
            {
                try
                {
                    payload = await ValidateAsync(bearerToken).ConfigureAwait(false);

                    await _tokenService.PersistPayloadToMemoryAndDb(bearerToken, ThirdPartyFrom.Google, payload)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Google authentication failed: {Exception}", ex.Message);
                    
                    return null;
                }
            }
            
            if (payload == null)
                return AuthenticateResult.NoResult();

            var principal =
                new ClaimsPrincipal(new ClaimsIdentity(GetClaims(payload), ThirdPartyFrom.Google.ToString()));

            SetupPrincipal(principal);
            
            return AuthenticateResult.Success(new AuthenticationTicket(principal,
                new AuthenticationProperties {IsPersistent = false}, Scheme.Name));
        }
    }
}