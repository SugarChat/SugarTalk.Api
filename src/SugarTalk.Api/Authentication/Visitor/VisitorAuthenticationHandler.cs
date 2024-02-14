using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SugarTalk.Core.Data.Claims;
using SugarTalk.Messages.Enums.Account;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SugarTalk.Api.Authentication.Visitor
{
    public class VisitorAuthenticationHandler : AuthenticationHandler<VisitorAuthenticationOptions>
    {
        public VisitorAuthenticationHandler(IOptionsMonitor<VisitorAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var userAccountType = Request.Headers[RequestHeaderKeys.UserAccountType].ToString();
            if (!string.IsNullOrWhiteSpace(userAccountType))
            {
                if (int.TryParse(userAccountType, out var userAccountTypeInt))
                {
                    if ((UserAccountType)userAccountTypeInt == UserAccountType.Visitor)
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
