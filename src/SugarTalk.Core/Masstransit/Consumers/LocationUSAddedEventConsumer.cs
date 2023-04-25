using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class LocationUSAddedEventConsumer : IFoundationEventConsumer<LocationUSAddedEvent>
    {
        private readonly IFoundationService _foundationService;

        public LocationUSAddedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<LocationUSAddedEvent> context)
        {
            await _foundationService.HandleLocationUSAddedEventAsync(context.Message, default)
                .ConfigureAwait(false);
        }
    }
}