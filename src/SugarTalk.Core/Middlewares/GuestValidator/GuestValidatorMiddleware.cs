using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using SugarTalk.Core.Services.Identity;

namespace SugarTalk.Core.Middlewares.GuestValidator;

public static class GuestValidatorMiddleware
{
    public static void UseGuestValidator<TContext>(
        this IPipeConfigurator<TContext> configurator, ICurrentUser currentUser = null) where TContext : IContext<IMessage>
    {
        if (currentUser == null && configurator.DependencyScope == null)
        {
            throw new DependencyScopeNotConfiguredException(
                $"{nameof(currentUser)} is not provided or IDependencyScope is not configured");
        }

        currentUser ??= configurator.DependencyScope.Resolve<ICurrentUser>();

        configurator.AddPipeSpecification(
            new GuestValidatorMiddlewareSpecification<TContext>(currentUser));
    }
}