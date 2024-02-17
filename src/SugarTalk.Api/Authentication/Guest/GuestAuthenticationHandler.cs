using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SugarTalk.Core.Constants;
using SugarTalk.Core.Data.Claims;
using SugarTalk.Core.Services.Account;
using SugarTalk.Messages.Enums.Account;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SugarTalk.Api.Authentication.Guest
{
    public class GuestAuthenticationHandler : AuthenticationHandler<GuestAuthenticationOptions>
    {
        private readonly IAccountDataProvider _accountDataProvider;

        public GuestAuthenticationHandler(IOptionsMonitor<GuestAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IAccountDataProvider accountDataProvider)
            : base(options, logger, encoder, clock)
        {
            _accountDataProvider = accountDataProvider;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey(RequestHeaderKeys.Issuer))
                return AuthenticateResult.NoResult();

            var issuser = Request.Headers[RequestHeaderKeys.Issuer].ToString();
            if (!string.IsNullOrWhiteSpace(issuser))
            {
                if (int.TryParse(issuser, out var issuserInt) && (UserAccountIssuer)issuserInt == UserAccountIssuer.Guest)
                {
                    var userName = Request.Headers.ContainsKey(RequestHeaderKeys.GuestSessionId) ? Request.Headers[RequestHeaderKeys.GuestSessionId].ToString() : Guid.NewGuid().ToString();
                    var account = await _accountDataProvider.CreateUserAccountAsync(userName,
                        string.Empty,
                        authType: UserAccountIssuer.Guest).ConfigureAwait(false);

                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, account.UserName),
                        new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                        new Claim(ClaimTypes.Authentication, UserAccountIssuer.Guest.ToString())
                    }, AuthenticationSchemeConstants.GuestAuthenticationScheme);
                    var claimPrincipal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(claimPrincipal, new AuthenticationProperties { IsPersistent = false }, Scheme.Name);
                    return AuthenticateResult.Success(ticket);
                }
            }

            return AuthenticateResult.NoResult();
        }
    }
}
