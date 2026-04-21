using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Serilog;
using SugarTalk.Core.Constants;
using SugarTalk.Core.Domain.OME;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Caching;
using SugarTalk.Core.Services.Http;
using SugarTalk.Core.Services.OME;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Enums.Caching;

namespace SugarTalk.Api.Authentication.OME;

public class OMEAuthenticationHandler: AuthenticationHandler<OMEAuthenticationOptions>
{
    private readonly ICacheManager _cacheManager;
    private readonly IAccountService _accountService;
    private readonly IOMEDataProvider _omeDataProvider;
    private readonly ISugarTalkHttpClientFactory _clientFactory;

    public OMEAuthenticationHandler(ICacheManager cacheManager, IOptionsMonitor<OMEAuthenticationOptions> options, ILoggerFactory logger, 
        UrlEncoder encoder, ISystemClock clock, IAccountService accountService, ISugarTalkHttpClientFactory clientFactory, IOMEDataProvider omeDataProvider)
        : base(options, logger, encoder, clock)
    {
        _cacheManager = cacheManager;
        _accountService = accountService;
        _clientFactory = clientFactory;
        _omeDataProvider = omeDataProvider;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Log.Information("OME Authentication start");
        
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.NoResult();

        var authorization = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer"))
            return AuthenticateResult.NoResult();
        
        if (string.IsNullOrWhiteSpace(Options.Authority)) return AuthenticateResult.NoResult();

        try
        {
            var userInfo = await _cacheManager.GetOrAddAsync(authorization, 
                async () => await GetUserFromAuthorizationServerAsync(authorization, default), CachingType.RedisCache,
                TimeSpan.FromDays(30)).ConfigureAwait(false);

            var id = Guid.TryParse(userInfo.Sub, out var userId) ? userId : Guid.Empty;

            if (string.IsNullOrWhiteSpace(userInfo.Sub) || id == Guid.Empty || string.IsNullOrWhiteSpace(userInfo.Username))
            {
                await _cacheManager.RemoveAsync(authorization, CachingType.RedisCache).ConfigureAwait(false);
                
                return AuthenticateResult.NoResult();
            }

            var userAccount = await _cacheManager.GetOrAddAsync(userInfo.Sub, async () => 
                await _accountService.GetOrCreateUserAccountFromThirdPartyAsync(userInfo.Sub, userInfo.Username, UserAccountIssuer.OME, default)
                    .ConfigureAwait(false), CachingType.RedisCache, TimeSpan.FromDays(30)).ConfigureAwait(false);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userAccount.UserName),
                new Claim(ClaimTypes.NameIdentifier, userAccount.Id.ToString()),
                new Claim(ClaimTypes.Authentication, UserAccountIssuer.OME.ToString())
            }, AuthenticationSchemeConstants.OMEAuthenticationScheme);

            userAccount.Roles.ForEach(x => identity.AddClaim(new Claim(ClaimTypes.Role, x.Name)));
            
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var authenticationTicket = new AuthenticationTicket(claimsPrincipal,
                new AuthenticationProperties { IsPersistent = false }, Scheme.Name);

            Request.HttpContext.User = claimsPrincipal;

            return AuthenticateResult.Success(authenticationTicket);
        }
        catch (Exception e)
        {
            Log.Warning("OME Authentication Warning: {@Warning}", e);

            return AuthenticateResult.NoResult();
        }
    }
    
    private async Task<OMEAuthorizationMessage> GetUserFromAuthorizationServerAsync(string authorization, CancellationToken cancellationToken)
    {
        var request = new Dictionary<string, string>
        {
            { "client_secret", Options.AppSecret },
            { "client_id", Options.AppId },
            { "token", authorization.Replace("Bearer ", "") },
            { "token_type_hint", "access_token" }
        };

        var content = new FormUrlEncodedContent(request);
        
        var response = await _clientFactory
            .PostAsync<OMEAuthorizationMessage>(Options.Authority, content, cancellationToken).ConfigureAwait(false);
        
        Log.Information("OME Authorization Response: {@Response}", response);

        if (response is not { Active: true }) return new OMEAuthorizationMessage();
        
        await PersistUserAsync(response, cancellationToken).ConfigureAwait(false);

        return response;
    }
    
    private async Task PersistUserAsync(OMEAuthorizationMessage message, CancellationToken cancellationToken)
    {
        var id = Guid.TryParse(message.Sub, out var userId) ? userId : Guid.Empty;

        if (id == Guid.Empty) return;

        var userAccount = new OMEUserAccount
        {
            Id = id,
            UserName = message.Username,
            NickName = message.Nickname,
            CreatedWay = message.CreatedWay,
            ExpireTime = message.Exp,
            Aud = string.Join(",", message.Aud)
        };

        await _omeDataProvider.AddUserAsync(userAccount, cancellationToken: cancellationToken).ConfigureAwait(false);
    }   
}
