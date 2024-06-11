using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SugarTalk.Api.Authentication.ApiKey;
using SugarTalk.Api.Authentication.Guest;
using SugarTalk.Api.Authentication.Wiltechs;
using SugarTalk.Core.Constants;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Settings.Authentication;

namespace SugarTalk.Api.Extensions;

public static class AuthenticationExtension
{
    public static void AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = false,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(new JwtSymmetricKeySetting(configuration).Value
                                .PadRight(256 / 8, '\0')))
                };
            })
            .AddScheme<WiltechsAuthenticationOptions, WiltechsAuthenticationHandler>(
                AuthenticationSchemeConstants.WiltechsAuthenticationScheme,
                options => options.Authority = configuration["Authentication:Wiltechs:Authority"])
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                AuthenticationSchemeConstants.ApiKeyAuthenticationScheme, _ => { })
            .AddScheme<GuestAuthenticationOptions, GuestAuthenticationHandler>(AuthenticationSchemeConstants.GuestAuthenticationScheme, op => { });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme,
                AuthenticationSchemeConstants.WiltechsAuthenticationScheme,
                AuthenticationSchemeConstants.ApiKeyAuthenticationScheme,
                AuthenticationSchemeConstants.GuestAuthenticationScheme).RequireAuthenticatedUser().Build();
        });
        
        services.AddScoped<ICurrentUser, CurrentUser>();
    }
}