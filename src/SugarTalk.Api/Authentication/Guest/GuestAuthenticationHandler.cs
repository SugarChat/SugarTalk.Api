using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SugarTalk.Core.Constants;
using SugarTalk.Core.Data;
using SugarTalk.Core.Data.Claims;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Extensions;
using SugarTalk.Messages.Enums.Account;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SugarTalk.Api.Authentication.Guest
{
    public class GuestAuthenticationHandler : AuthenticationHandler<GuestAuthenticationOptions>
    {
        private readonly SugarTalkDbContext _dbContext;

        public GuestAuthenticationHandler(IOptionsMonitor<GuestAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, SugarTalkDbContext dbContext)
            : base(options, logger, encoder, clock)
        {
            _dbContext = dbContext;
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
                        var userAccount = new UserAccount
                        {
                            CreatedOn = DateTime.Now,
                            ModifiedOn = DateTime.Now,
                            Uuid = Guid.NewGuid(),
                            UserName = Guid.NewGuid().ToString(),
                            Password = string.Empty.ToSha256(),
                            ThirdPartyUserId = Guid.NewGuid().ToString(),
                            Issuer = UserAccountIssuer.Guest,
                            IsActive = true
                        };
                        await _dbContext.AddAsync(userAccount);
                        await _dbContext.SaveChangesAsync();
                        var identity = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name, userAccount.UserName),
                            new Claim(ClaimTypes.NameIdentifier, userAccount.Id.ToString()),
                            new Claim(ClaimTypes.Authentication, UserAccountIssuer.Wiltechs.ToString())
                        }, AuthenticationSchemeConstants.GuestAuthenticationScheme);
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
