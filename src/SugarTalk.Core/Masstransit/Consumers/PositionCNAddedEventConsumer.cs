using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class PositionCNAddedEventConsumer : IFoundationEventConsumer<PositionCNAddedEvent>
    {
        private readonly IFoundationService _foundationService;

        public PositionCNAddedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<PositionCNAddedEvent> context)
        {
            await _foundationService.HandlePositionCNAddedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}