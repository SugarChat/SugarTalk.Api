using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using SugarTalk.Core.Services.Identity;

namespace SugarTalk.Core.Middlewares.GuestValidator;

public static class GuestValidatorMiddleware
{
    public static void UseGuestValidator<TContext>(this IPipeConfigurator<TContext> configurator,
        ICurrentUser currentUser = null, IIdentityService identityService = null)
        where TContext : IContext<IMessage>
    {
        if ((currentUser == null || identityService == null) && configurator.DependencyScope == null)
        {
            throw new DependencyScopeNotConfiguredException(
                $"{nameof(identityService)} or {nameof(currentUser)} is not provided and IDependencyScope is not configured, Please ensure {nameof(identityService)} or {nameof(currentUser)} is registered properly if you are using IoC container, otherwise please pass {nameof(identityService)} as parameter");
        }

        currentUser ??= configurator.DependencyScope.Resolve<ICurrentUser>();
        
        identityService ??= configurator.DependencyScope.Resolve<IIdentityService>();

        configurator.AddPipeSpecification(
            new GuestValidatorMiddlewareSpecification<TContext>(currentUser, identityService));
    }
}