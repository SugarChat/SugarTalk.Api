using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class StaffCNAddedEventConsumer : IFoundationEventConsumer<StaffCNAddedEvent>
    {
        private readonly IFoundationService _foundationService;

        public StaffCNAddedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<StaffCNAddedEvent> context)
        {
            await _foundationService.HandleStaffCNAddedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}