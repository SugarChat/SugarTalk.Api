using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class UserAccountCNAddedEventConsumer : IFoundationEventConsumer<UserAccountCNAddedEvent>
    {
        private readonly IFoundationService _foundationService;

        public UserAccountCNAddedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<UserAccountCNAddedEvent> context)
        {
            await _foundationService.HandleUserAccountCNAddedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}