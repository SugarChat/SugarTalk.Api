using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class LocationUSDeletedEventConsumer : IFoundationEventConsumer<LocationUSDeletedEvent>
    {
        private readonly IFoundationService _foundationService;

        public LocationUSDeletedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<LocationUSDeletedEvent> context)
        {
            await _foundationService.HandleLocationUSDeletedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}