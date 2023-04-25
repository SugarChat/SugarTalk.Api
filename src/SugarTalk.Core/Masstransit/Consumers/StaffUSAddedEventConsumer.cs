using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class StaffUSAddedEventConsumer : IFoundationEventConsumer<StaffUSAddedEvent>
    {
        private readonly IFoundationService _foundationService;

        public StaffUSAddedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<StaffUSAddedEvent> context)
        {
            await _foundationService.HandleStaffUSAddedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}