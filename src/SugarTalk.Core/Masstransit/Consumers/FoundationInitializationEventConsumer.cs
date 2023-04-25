using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class FoundationInitializationEventConsumer : IFoundationEventConsumer<FoundationInitializationEvent>
    {
        private readonly IFoundationService _foundationService;
        
        public FoundationInitializationEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<FoundationInitializationEvent> context)
        {
            await _foundationService.HandleFoundationInitializationEventAsync(context.Message, default)
                .ConfigureAwait(false);
        }
    }
}