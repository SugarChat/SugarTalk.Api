using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SugarTalk.Api.Authentication.Facebook;
using SugarTalk.Api.Authentication.Wechat;
using SugarTalk.Messages;
using SugarTalk.Messages.Enums;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace SugarTalk.Api.Authentication
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
            var name = payload.Name ?? "";
            var email = payload.Email ?? "";
            var picture = payload.Picture ?? "";
            var thirdPartyId = payload.Subject;
            
            return new List<Claim>
            {
                new(ClaimTypes.Name, name),
                new(ClaimTypes.Email, email),
                new(SugarTalkConstants.Picture, picture),
                new(SugarTalkConstants.ThirdPartyId, thirdPartyId),
                new(SugarTalkConstants.ThirdPartyFrom, ThirdPartyFrom.Google.ToString())
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
                new(SugarTalkConstants.Picture, picture),
                new(SugarTalkConstants.ThirdPartyId, thirdPartyId),
                new(SugarTalkConstants.ThirdPartyFrom, ThirdPartyFrom.Facebook.ToString())
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
                new(SugarTalkConstants.Picture, picture),
                new(SugarTalkConstants.ThirdPartyId, thirdPartyId),
                new(SugarTalkConstants.ThirdPartyFrom, ThirdPartyFrom.Wechat.ToString())
            };
        }
    }
}