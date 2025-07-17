using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using SugarTalk.Core.Services.Caching;

namespace SugarTalk.Core.Middlewares.RequestCaching;

public static class RequestCachingMiddleware
{
    public static void UseRequestCaching<TContext>(this IPipeConfigurator<TContext> configurator, ICacheManager cacheManager = null) where TContext : IContext<IMessage>
    {
        if (configurator.DependencyScope == null)
            throw new DependencyScopeNotConfiguredException(nameof(configurator.DependencyScope));

        cacheManager ??= configurator.DependencyScope.Resolve<ICacheManager>();
        
        configurator.AddPipeSpecification(new RequestCachingSpecification<TContext>(cacheManager));
    }
}