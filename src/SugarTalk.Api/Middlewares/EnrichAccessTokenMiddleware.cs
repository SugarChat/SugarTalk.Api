using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SugarTalk.Api.Middlewares
{
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

            // web sockets cannot pass headers so we must take the access token from query param and
            // add it to the header before authentication middleware runs
            if (request.Path.StartsWithSegments("/meetingHub", StringComparison.OrdinalIgnoreCase) &&
                request.Query.TryGetValue("access_token", out var accessToken))
            {
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
            }

            await _next.Invoke(context);
        }
    }
}