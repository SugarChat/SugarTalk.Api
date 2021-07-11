using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SugarTalk.Core;
using SugarTalk.Messages.Enums;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace SugarTalk.Api.Middlewares.Authentication
{
    public abstract class AuthenticationHandlerBase<TOptions> : AuthenticationHandler<TOptions> where TOptions : AuthenticationSchemeOptions, new()
    {
        protected AuthenticationHandlerBase(IOptionsMonitor<TOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected void SetupPrincipal(ClaimsPrincipal principal)
        {
            Request.HttpContext.User = principal;
        }
        
        protected IEnumerable<Claim> GetClaims(Payload payload)
        {
            var name = payload.Name;
            var email = payload.Email ?? "";
            var picture = payload.Picture ?? "";
            var thirdPartyId = payload.Subject;
            
            return new List<Claim>
            {
                new(ClaimTypes.Name, name),
                new(ClaimTypes.Email, email),
                new(SugarTalkClaimType.Picture, picture),
                new(SugarTalkClaimType.ThirdPartyId, thirdPartyId),
                new(SugarTalkClaimType.ThirdPartyFrom, ThirdPartyFrom.Google.ToString())
            };
        }
        
        protected IEnumerable<Claim> GetClaims(FacebookPayload payload)
        {
            var name = payload.Name;
            var email = payload.Email ?? "";
            var picture = payload.Picture?.Data?.Url ?? "";
            var thirdPartyId = payload.Id;
            
            return new List<Claim>
            {
                new(ClaimTypes.Name, name),
                new(ClaimTypes.Email, email),
                new(SugarTalkClaimType.Picture, picture),
                new(SugarTalkClaimType.ThirdPartyId, thirdPartyId),
                new(SugarTalkClaimType.ThirdPartyFrom, ThirdPartyFrom.Facebook.ToString())
            };
        }
        
        protected IEnumerable<Claim> GetClaims(WechatPayload payload)
        {
            var name = payload.NickName;
            var email = payload.OpenId ?? "";
            var picture = payload.HeadImgUrl ?? "";
            var thirdPartyId = payload.UnionId;
            
            return new List<Claim>
            {
                new(ClaimTypes.Name, name),
                new(ClaimTypes.Email, email),
                new(SugarTalkClaimType.Picture, picture),
                new(SugarTalkClaimType.ThirdPartyId, thirdPartyId),
                new(SugarTalkClaimType.ThirdPartyFrom, ThirdPartyFrom.Wechat.ToString())
            };
        }
    }
}