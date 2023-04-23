using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class UnitUSUpdatedEventConsumer : IFoundationEventConsumer<UnitUSUpdatedEvent>
    {
        private readonly IFoundationService _foundationService;

        public UnitUSUpdatedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<UnitUSUpdatedEvent> context)
        {
            await _foundationService.HandleUnitUSUpdatedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}