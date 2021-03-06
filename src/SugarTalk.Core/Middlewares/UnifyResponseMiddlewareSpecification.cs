using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using SugarTalk.Messages;

namespace SugarTalk.Core.Middlewares
{
    public class UnifyResponseMiddlewareSpecification<TContext> : IPipeSpecification<TContext>
         where TContext : IContext<IMessage>
    {
        public Task AfterExecute(TContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task BeforeExecute(TContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task Execute(TContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OnException(Exception ex, TContext context)
        {
            if (ex is not BusinessException businessException || context.Message is IEvent)
            {
                ExceptionDispatchInfo.Capture(ex).Throw();
                throw ex;
            }

            if (context.Result is null)
            {
                var unifiedTypeInstance = Activator.CreateInstance(context.ResultDataType);
                context.Result = unifiedTypeInstance;
            }

            if (context.Result is ISugarTalkResponse response)
            {
                response.Code = businessException.Code;
                response.Message = businessException.Message;
            }
            return Task.CompletedTask;
        }

        public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
        {
            return true;
        }
    }
}