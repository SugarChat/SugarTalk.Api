using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class StaffUSUpdatedEventConsumer : IFoundationEventConsumer<StaffUSUpdatedEvent>
    {
        private readonly IFoundationService _foundationService;

        public StaffUSUpdatedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<StaffUSUpdatedEvent> context)
        {
            await _foundationService.HandleStaffUSUpdatedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}