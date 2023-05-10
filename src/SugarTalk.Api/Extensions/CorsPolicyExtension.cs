using SugarTalk.Core.Settings.CorsPolicy;

namespace SugarTalk.Api.Extensions;

public static class CorsPolicyExtension
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.WithOrigins(new AllowableCorsOriginsSetting(configuration).Value)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });
        
        return services;
    }
}