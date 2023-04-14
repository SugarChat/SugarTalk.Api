using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SugarTalk.Api.Authentication.Facebook;
using SugarTalk.Api.Authentication.Google;
using SugarTalk.Api.Authentication.Wechat;
using SugarTalk.Core.Constants;

namespace SugarTalk.Api.Extensions;

public static class AuthenticationExtension
{
    public static void AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddScheme<GoogleAuthenticationOptions, GoogleAuthenticationHandler>(
                AuthenticationSchemeConstants.GoogleAuthenticationScheme, _ => { })
            .AddScheme<WechatAuthenticationOptions, WechatAuthenticationHandler>(
                AuthenticationSchemeConstants.WechatAuthenticationScheme, _ => { })
            .AddScheme<FacebookAuthenticationOptions, FacebookAuthenticationHandler>(
                AuthenticationSchemeConstants.FacebookAuthenticationScheme, _ => { });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(
                AuthenticationSchemeConstants.GoogleAuthenticationScheme, 
                AuthenticationSchemeConstants.WechatAuthenticationScheme, 
                AuthenticationSchemeConstants.FacebookAuthenticationScheme).RequireAuthenticatedUser().Build();
        });
    }
}