using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SugarTalk.Core;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Api.Middlewares.Authentication
{
    public class WechatAuthenticationHandler : AuthenticationHandler<WechatAuthenticationOptions>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        public WechatAuthenticationHandler(IOptionsMonitor<WechatAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, IHttpClientFactory httpClientFactory) : base(options, logger, encoder, clock)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.NoResult();
            
            var auth = Request.Headers["Authorization"].ToString();
            
            if (string.IsNullOrWhiteSpace(auth) || !auth.Contains("Bearer") || !auth.Contains("OpenId"))
                return AuthenticateResult.NoResult();
            
            var authHeaderValue = auth.Split(' ');
            
            if (authHeaderValue.Length != 4)
                return AuthenticateResult.NoResult();
            
            var bearerToken = authHeaderValue[1];
            var openId = authHeaderValue[3];
            
            if (string.IsNullOrEmpty(bearerToken) || string.IsNullOrEmpty(openId) || bearerToken.Contains("undefined"))
                return AuthenticateResult.NoResult();

            var wechatUserInfoUrl =
                $"https://api.weixin.qq.com/sns/userinfo?access_token={bearerToken}&openid={openId}&lang=zh_CN";

            var payloadJsonStr = await _httpClientFactory.CreateClient().GetStringAsync(wechatUserInfoUrl)
                .ConfigureAwait(false);

            if (payloadJsonStr.Contains("errcode")) return AuthenticateResult.NoResult();
            
            var payload = JsonConvert.DeserializeObject<WechatPayload>(payloadJsonStr);

            return AuthenticateResult.Success(new AuthenticationTicket
            (
                new ClaimsPrincipal(new ClaimsIdentity(GetClaims(payload), "Wechat")
            ), new AuthenticationProperties {IsPersistent = false}, Scheme.Name));
        }
        
        private IEnumerable<Claim> GetClaims(WechatPayload payload)
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