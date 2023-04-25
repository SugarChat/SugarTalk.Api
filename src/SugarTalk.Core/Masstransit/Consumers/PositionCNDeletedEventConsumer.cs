using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class PositionCNDeletedEventConsumer : IFoundationEventConsumer<PositionCNDeletedEvent>
    {
        private readonly IFoundationService _foundationService;

        public PositionCNDeletedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<PositionCNDeletedEvent> context)
        {
            await _foundationService.HandlePositionCNDeletedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}