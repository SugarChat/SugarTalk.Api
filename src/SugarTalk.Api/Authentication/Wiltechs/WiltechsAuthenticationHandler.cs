using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SugarTalk.Core.Constants;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Caching;
using SugarTalk.Core.Services.Http;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Enums.Caching;

namespace SugarTalk.Api.Authentication.Wiltechs;

public class WiltechsAuthenticationHandler : AuthenticationHandler<WiltechsAuthenticationOptions>
{
    private readonly IAccountService _userService;
    private readonly ICacheManager _cacheManager;
    private readonly ISugarTalkHttpClientFactory _clientFactory;

    public WiltechsAuthenticationHandler(ICacheManager cacheManager, IOptionsMonitor<WiltechsAuthenticationOptions> options, 
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IAccountService userService, ISugarTalkHttpClientFactory clientFactory)
        : base(options, logger, encoder, clock)
    {
        _userService = userService;
        _cacheManager = cacheManager;
        _clientFactory = clientFactory;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.NoResult();

        var authorization = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer"))
            return AuthenticateResult.NoResult();

        if (IsJwtToken(authorization)) return AuthenticateResult.NoResult();
        
        try
        {
            var wiltechUser = await _cacheManager.GetOrAddAsync(authorization, async () =>
            {
                var headers = new Dictionary<string, string> { { "Authorization", authorization } };

                return await _clientFactory
                    .GetAsync<WiltechsUserInfo>(Options.Authority, CancellationToken.None, headers: headers).ConfigureAwait(false);
            }, CachingType.RedisCache, TimeSpan.FromDays(30), CancellationToken.None).ConfigureAwait(false);

            if (wiltechUser.UserId == Guid.Empty && string.IsNullOrWhiteSpace(wiltechUser.UserName))
            {
                return AuthenticateResult.NoResult();
            }

            var userAccount = await _cacheManager.GetOrAddAsync(wiltechUser.UserId.ToString(), async () => 
                await _userService.GetOrCreateUserAccountFromThirdPartyAsync(wiltechUser.UserId.ToString(), wiltechUser.UserName, CancellationToken.None)
                    .ConfigureAwait(false), CachingType.RedisCache, TimeSpan.FromDays(30), CancellationToken.None).ConfigureAwait(false);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userAccount.UserName),
                new Claim(ClaimTypes.NameIdentifier, userAccount.ThirdPartyUserId),
                new Claim(ClaimTypes.Authentication, UserAccountIssuer.Wiltechs.ToString())
            }, AuthenticationSchemeConstants.WiltechsAuthenticationScheme);

            userAccount.Roles.ForEach(x => identity.AddClaim(new Claim(ClaimTypes.Role, x.Name)));
            
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var authenticationTicket = new AuthenticationTicket(claimsPrincipal,
                new AuthenticationProperties { IsPersistent = false }, Scheme.Name);

            Request.HttpContext.User = claimsPrincipal;

            return AuthenticateResult.Success(authenticationTicket);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private static bool IsJwtToken(string authorization)
    {
        var token = authorization.Replace("Bearer ", "");
        
        var tokenHandler = new JwtSecurityTokenHandler();
        
        return tokenHandler.CanReadToken(token);
    }
}