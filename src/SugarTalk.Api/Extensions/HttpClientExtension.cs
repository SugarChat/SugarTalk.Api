using Correlate;
using Microsoft.Extensions.DependencyInjection;
using SugarTalk.Messages;

namespace SugarTalk.Api.Extensions;

public static class HttpClientExtension
{
    public static void AddHttpClientInternal(this IServiceCollection services)
    {
        services.AddHttpClient(string.Empty, (sp, c) =>
        {
            var correlationContextAccessor = sp.GetRequiredService<ICorrelationContextAccessor>();

            if (correlationContextAccessor.CorrelationContext == null) return;

            foreach (var correlationIdHeader in SugarTalkConstants.CorrelationIdHeaders)
            {
                c.DefaultRequestHeaders.Add(correlationIdHeader, correlationContextAccessor.CorrelationContext.CorrelationId);
            }
        });
    }
}