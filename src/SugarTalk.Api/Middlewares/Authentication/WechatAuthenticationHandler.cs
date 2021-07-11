using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using SugarTalk.Core;
using SugarTalk.Core.Services.Authentication;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Api.Middlewares.Authentication
{
    public class WechatAuthenticationHandler : AuthenticationHandlerBase<WechatAuthenticationOptions>
    {
        private readonly ITokenService _tokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        
        public WechatAuthenticationHandler(IOptionsMonitor<WechatAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, IHttpClientFactory httpClientFactory, ITokenService tokenService) : base(options, logger, encoder, clock)
        {
            _tokenService = tokenService;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.HttpContext.User.Identity == null || Request.HttpContext.User.Identity.IsAuthenticated)
                return AuthenticateResult.NoResult();
            
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

            var payload = await _tokenService
                .GetPayloadFromMemoryOrDb<WechatPayload>(openId, ThirdPartyFrom.Wechat)
                .ConfigureAwait(false);

            if (payload == null)
            {
                var wechatUserInfoUrl =
                    $"https://api.weixin.qq.com/sns/userinfo?access_token={bearerToken}&openid={openId}&lang=zh_CN";

                try
                {
                    payload = await _httpClientFactory.CreateClient().GetFromJsonAsync<WechatPayload>(wechatUserInfoUrl)
                        .ConfigureAwait(false);
                    
                    await _tokenService.PersistPayloadToMemoryAndDb(openId, ThirdPartyFrom.Wechat, payload)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Wechat authentication failed: {Exception}", ex.Message);
                    
                    return null;
                }
            }

            if (payload == null)
                return AuthenticateResult.NoResult();

            var principal =
                new ClaimsPrincipal(new ClaimsIdentity(GetClaims(payload), ThirdPartyFrom.Wechat.ToString()));
            
            SetupPrincipal(principal);

            return AuthenticateResult.Success(new AuthenticationTicket(principal,
                new AuthenticationProperties {IsPersistent = false}, Scheme.Name));
        }
    }
}