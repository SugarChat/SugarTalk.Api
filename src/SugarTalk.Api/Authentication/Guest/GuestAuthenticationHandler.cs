using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SugarTalk.Core.Constants;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Caching;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Account;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SugarTalk.Api.Authentication.Guest
{
    public class GuestAuthenticationHandler : AuthenticationHandler<GuestAuthenticationOptions>
    {
        private readonly IAccountDataProvider _accountDataProvider;
        private readonly IMapper _mapper;
        private readonly ICacheManager _cacheManager;

        public GuestAuthenticationHandler(IOptionsMonitor<GuestAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            IAccountDataProvider accountDataProvider, IMapper mapper, ICacheManager cacheManager)
            : base(options, logger, encoder, clock)
        {
            _accountDataProvider = accountDataProvider;
            _mapper = mapper;
            _cacheManager = cacheManager;
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
                    UserAccountDto accountDto = null;
                    var guestSessionId = Request.Headers.ContainsKey(RequestHeaderKeys.GuestSessionId) ? Request.Headers[RequestHeaderKeys.GuestSessionId].ToString() : null;
                    if (!string.IsNullOrWhiteSpace(guestSessionId))
                    {
                        accountDto = await _cacheManager.GetOrAddAsync(CacheService.GenerateUserAccountByUserNameKey(guestSessionId), async () =>
                            await _accountDataProvider.GetUserAccountAsync(username: guestSessionId).ConfigureAwait(false), Messages.Enums.Caching.CachingType.RedisCache);
                    }

                    if (accountDto == default)
                    {
                        var userName = Request.Headers.ContainsKey(RequestHeaderKeys.GuestSessionId) ? Request.Headers[RequestHeaderKeys.GuestSessionId].ToString() : Guid.NewGuid().ToString();
                        var account = await _accountDataProvider.CreateUserAccountAsync(userName,
                            string.Empty,
                            authType: UserAccountIssuer.Guest).ConfigureAwait(false);
                        accountDto = _mapper.Map<UserAccountDto>(account);
                    }

                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, accountDto.UserName),
                        new Claim(ClaimTypes.NameIdentifier, accountDto.Id.ToString()),
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
