using MassTransit;

namespace SugarTalk.Core.Masstransit.Consumers;

public interface IFoundationEventConsumer<in T> : IConsumer<T> where T : class
{
}
