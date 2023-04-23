using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class PositionUSDeletedEventConsumer : IFoundationEventConsumer<PositionUSDeletedEvent>
    {
        private readonly IFoundationService _foundationService;

        public PositionUSDeletedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<PositionUSDeletedEvent> context)
        {
            await _foundationService.HandlePositionUSDeletedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}