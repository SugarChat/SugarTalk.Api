using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class UnitCNDeletedEventConsumer : IFoundationEventConsumer<UnitCNDeletedEvent>
    {
        private readonly IFoundationService _foundationService;

        public UnitCNDeletedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<UnitCNDeletedEvent> context)
        {
            await _foundationService.HandleUnitCNDeletedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}