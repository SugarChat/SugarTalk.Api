using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;

namespace SugarTalk.Core.Middlewares
{
    public static class MiddlewareExtension
    {
        public static void UseUnifyResponseMiddleware<TContext>(this IPipeConfigurator<TContext> configurator)
            where TContext : IContext<IMessage>
        {
            configurator.AddPipeSpecification(new UnifyResponseMiddlewareSpecification<TContext>());
        }
    }
}