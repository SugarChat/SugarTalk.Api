using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SugarTalk.Core.Data.Claims;
using SugarTalk.Messages.Enums.Account;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SugarTalk.Api.Authentication.Guest
{
    public class GuestAuthenticationHandler : AuthenticationHandler<GuestAuthenticationOptions>
    {
        public GuestAuthenticationHandler(IOptionsMonitor<GuestAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var issuser = Request.Headers[RequestHeaderKeys.Issuer].ToString();
            if (!string.IsNullOrWhiteSpace(issuser))
            {
                if (int.TryParse(issuser, out var issuserInt))
                {
                    if ((UserAccountIssuer)issuserInt == UserAccountIssuer.Guest)
                    {
                        var identity = new ClaimsIdentity("Visitor");
                        var claimPrincipal = new ClaimsPrincipal(identity);
                        var ticket = new AuthenticationTicket(claimPrincipal, new AuthenticationProperties { IsPersistent = false }, Scheme.Name);
                        return AuthenticateResult.Success(ticket);
                    }
                }
            }

            return AuthenticateResult.NoResult();
        }
    }
}
