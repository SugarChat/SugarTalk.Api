using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class StaffCNUpdatedEventConsumer : IFoundationEventConsumer<StaffCNUpdatedEvent>
    {
        private readonly IFoundationService _foundationService;

        public StaffCNUpdatedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<StaffCNUpdatedEvent> context)
        {
            await _foundationService.HandleStaffCNUpdatedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}