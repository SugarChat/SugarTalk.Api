using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class UnitUSAddedEventConsumer : IFoundationEventConsumer<UnitUSAddedEvent>
    {
        private readonly IFoundationService _foundationService;

        public UnitUSAddedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<UnitUSAddedEvent> context)
        {
            await _foundationService.HandleUnitUSAddedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}