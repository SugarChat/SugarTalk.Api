using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class UnitCNUpdatedEventConsumer : IFoundationEventConsumer<UnitCNUpdatedEvent>
    {
        private readonly IFoundationService _foundationService;

        public UnitCNUpdatedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<UnitCNUpdatedEvent> context)
        {
            await _foundationService.HandleUnitCNUpdatedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}