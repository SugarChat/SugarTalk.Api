using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Constants;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Services.Identity;

public interface ICurrentUser
{
    Guid Id { get; }
    
    ThirdPartyFrom AuthType { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid Id
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
                throw new ApplicationException("HttpContext is not available");

            var idClaim = _httpContextAccessor.HttpContext.User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier);

            return Guid.Parse(idClaim.Value);
        }
    }

    public ThirdPartyFrom AuthType
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
                throw new ApplicationException("HttpContext is not available");

            return _httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Authentication)?.Value switch
            {
                AuthenticationSchemeConstants.GoogleAuthenticationScheme => ThirdPartyFrom.Google,
                AuthenticationSchemeConstants.WechatAuthenticationScheme => ThirdPartyFrom.Wechat,
                AuthenticationSchemeConstants.FacebookAuthenticationScheme => ThirdPartyFrom.Facebook
            };
        }
    }
}
