using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class PositionUSAddedEventConsumer : IFoundationEventConsumer<PositionUSAddedEvent>
    {
        private readonly IFoundationService _foundationService;

        public PositionUSAddedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<PositionUSAddedEvent> context)
        {
            await _foundationService.HandlePositionUSAddedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}