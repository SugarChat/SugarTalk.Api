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
using Serilog;
using SugarTalk.Messages.Enums.Caching;

namespace SugarTalk.Api.Authentication.Guest
{
    public class GuestAuthenticationHandler : AuthenticationHandler<GuestAuthenticationOptions>
    {
        private readonly IMapper _mapper;
        private readonly ICacheManager _cacheManager;
        private readonly IAccountService _accountService;
        
        public GuestAuthenticationHandler(IOptionsMonitor<GuestAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
           IMapper mapper, ICacheManager cacheManager, IAccountService accountService)
            : base(options, logger, encoder, clock)
        {
            _mapper = mapper;
            _cacheManager = cacheManager;
            _accountService = accountService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey(RequestHeaderKeys.GuestSessionId)) return AuthenticateResult.NoResult();
            
            var guestSessionId = Request.Headers[RequestHeaderKeys.GuestSessionId].ToString();

            Log.Information("Request headers for Guest Session Id: {GuestSessionId}", guestSessionId);
            
            if (string.IsNullOrEmpty(guestSessionId)) return AuthenticateResult.NoResult();
            
            var accountDto = await _cacheManager.GetOrAddAsync(guestSessionId, async () => 
                await _accountService.GetOrCreateGuestUserAccountAsync(guestSessionId, CancellationToken.None).ConfigureAwait(false),
                CachingType.RedisCache, TimeSpan.FromDays(30), CancellationToken.None).ConfigureAwait(false);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, accountDto.UserName),
                new Claim(ClaimTypes.NameIdentifier, accountDto.Id.ToString()),
                new Claim(ClaimTypes.Authentication, UserAccountIssuer.Guest.ToString())
            }, AuthenticationSchemeConstants.GuestAuthenticationScheme);

            var claimPrincipal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(claimPrincipal, new AuthenticationProperties { IsPersistent = false }, Scheme.Name);

            Request.HttpContext.User = claimPrincipal;
            
            return AuthenticateResult.Success(ticket);
        }
    }
}
