namespace SugarTalk.Api.Middlewares;

public class EnrichAccessTokenMiddleware
{
    private readonly RequestDelegate _next;

    public EnrichAccessTokenMiddleware(RequestDelegate requestDelegate)
    {
        _next = requestDelegate;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        
        if (request.Path.StartsWithSegments("/meetingHub", StringComparison.OrdinalIgnoreCase) &&
            request.Query.TryGetValue("access_token", out var accessToken))
        {
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
        }

        await _next.Invoke(context);
    }
}
