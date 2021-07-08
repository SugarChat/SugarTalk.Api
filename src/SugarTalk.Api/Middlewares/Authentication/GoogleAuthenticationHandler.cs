using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SugarTalk.Core;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace SugarTalk.Api.Middlewares.Authentication
{
    public class GoogleAuthenticationHandler : AuthenticationHandler<GoogleAuthenticationOptions>
    {
        public GoogleAuthenticationHandler(IOptionsMonitor<GoogleAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
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

            Payload payload;
            
            try
            {
                payload = await ValidateAsync(bearerToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                //TODO. LOG
                return null;
            }
            
            if (payload == null)
                return AuthenticateResult.NoResult();

            return AuthenticateResult.Success(new AuthenticationTicket
            (
                new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new(ClaimTypes.Name, payload.Name),
                    new(SugarTalkClaimType.Picture, payload.Picture),
                    new(SugarTalkClaimType.ThirdPartyId, payload.Subject)
                }, "Google")
            ), new AuthenticationProperties {IsPersistent = false}, Scheme.Name));
        }
    }
}