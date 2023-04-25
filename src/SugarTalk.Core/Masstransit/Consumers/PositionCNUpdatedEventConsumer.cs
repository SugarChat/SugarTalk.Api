using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class PositionCNUpdatedEventConsumer : IFoundationEventConsumer<PositionCNUpdatedEvent>
    {
        private readonly IFoundationService _foundationService;

        public PositionCNUpdatedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<PositionCNUpdatedEvent> context)
        {
            await _foundationService.HandlePositionCNUpdatedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}