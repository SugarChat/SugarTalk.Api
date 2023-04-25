using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class UnitCNAddedEventConsumer : IFoundationEventConsumer<UnitCNAddedEvent>
    {
        private readonly IFoundationService _foundationService;

        public UnitCNAddedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<UnitCNAddedEvent> context)
        {
            await _foundationService.HandleUnitCNAddedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}