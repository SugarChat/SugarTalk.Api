using System.Threading.Tasks;
using HR.Message.Contract.Event;
using MassTransit;
using SugarTalk.Core.Services.Foundation;

namespace SugarTalk.Core.Masstransit.Consumers
{
    public class UserAccountUSAddedEventConsumer : IFoundationEventConsumer<UserAccountUSAddedEvent>
    {
        private readonly IFoundationService _foundationService;

        public UserAccountUSAddedEventConsumer(IFoundationService foundationService)
        {
            _foundationService = foundationService;
        }

        public async Task Consume(ConsumeContext<UserAccountUSAddedEvent> context)
        {
           await _foundationService.HandleUserAccountUSAddedEventAsync(context.Message, default).ConfigureAwait(false);
        }
    }
}