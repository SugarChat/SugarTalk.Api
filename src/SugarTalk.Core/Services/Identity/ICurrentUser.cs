using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Constants;
using SugarTalk.Messages.Enums.Account;

namespace SugarTalk.Core.Services.Identity;

public interface ICurrentUser
{
    int Id { get; }
    
    UserAccountIssuer AuthType { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int Id
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
                throw new ApplicationException("HttpContext is not available");

            var idClaim = _httpContextAccessor.HttpContext.User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier);

            return int.Parse(idClaim.Value);
        }
    }

    public UserAccountIssuer AuthType
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
                throw new ApplicationException("HttpContext is not available");

            return _httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Authentication)?.Value switch
            {
                AuthenticationSchemeConstants.SelfAuthenticationScheme => UserAccountIssuer.Self,
                AuthenticationSchemeConstants.WiltechsAuthenticationScheme => UserAccountIssuer.Wiltechs
            };
        }
    }
}

public class InternalUser : ICurrentUser
{
    public int Id => 1;

    public UserAccountIssuer AuthType => UserAccountIssuer.Wiltechs;
}