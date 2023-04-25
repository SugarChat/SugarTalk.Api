using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class UnitCNMovedEventConsumer : IFoundationEventConsumer<UnitCNMovedEvent>
    {
        private readonly IFoundationService _foundationService;

        public UnitCNMovedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<UnitCNMovedEvent> context)
        {
            await _foundationService.HandleUnitCNMovedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}