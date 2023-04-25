using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class LocationUSUpdatedEventConsumer : IFoundationEventConsumer<LocationUSUpdatedEvent>
    {
        private readonly IFoundationService _foundationService;

        public LocationUSUpdatedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<LocationUSUpdatedEvent> context)
        {
            await _foundationService.HandleLocationUSUpdatedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}