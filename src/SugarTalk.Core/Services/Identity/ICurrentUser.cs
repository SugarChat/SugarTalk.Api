using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Constants;
using SugarTalk.Messages.Constants;
using SugarTalk.Messages.Enums.Account;

namespace SugarTalk.Core.Services.Identity;

public interface ICurrentUser
{
    int? Id { get; }
    
    Guid? UserId { get; }
    
    UserAccountIssuer AuthType { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? Id
    {
        get
        {
            if (_httpContextAccessor?.HttpContext == null) return null;

            var idClaim = _httpContextAccessor.HttpContext.User.Claims
                .SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public Guid? UserId
    {
        get
        {
            return _httpContextAccessor.HttpContext == null
                ? throw new ApplicationException("HttpContext is not available")
                : Guid.Parse(_httpContextAccessor.HttpContext.User.Claims.Single(x => x.Type == ClaimTypes.SerialNumber).Value);
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
    public int? Id => CurrentUsers.InternalUser.Id;
    
    public Guid? UserId => CurrentUsers.InternalUser.UserId;

    public UserAccountIssuer AuthType => UserAccountIssuer.Wiltechs;
}