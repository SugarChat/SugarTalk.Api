using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Services.Identity;

namespace SugarTalk.Core.Middlewares.Authorization;

public static class AuthorizationMiddleware
{
    public static void UseAuthorization<TContext>(
        this IPipeConfigurator<TContext> configurator,
        ICurrentUser currentUser = null,
        IIdentityService identityService = null,
        IHttpContextAccessor httpContextAccessor = null)
        where TContext : IContext<IMessage>
    {
        if ((currentUser == null || identityService == null || httpContextAccessor == null) &&
            configurator.DependencyScope == null)
            throw new DependencyScopeNotConfiguredException("Authorization dependencies are not configured.");

        currentUser ??= configurator.DependencyScope.Resolve<ICurrentUser>();
        identityService ??= configurator.DependencyScope.Resolve<IIdentityService>();
        httpContextAccessor ??= configurator.DependencyScope.Resolve<IHttpContextAccessor>();

        configurator.AddPipeSpecification(
            new AuthorizationMiddlewareSpecification<TContext>(currentUser, identityService, httpContextAccessor));
    }
}
