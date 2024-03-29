using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings
{
    public class ScheduleMeetingCommandHandler: ICommandHandler<ScheduleMeetingCommand, ScheduleMeetingResponse>
    {
        private readonly IMeetingService _meetingService;

        public ScheduleMeetingCommandHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }
        
        public async Task<ScheduleMeetingResponse> Handle(IReceiveContext<ScheduleMeetingCommand> context, CancellationToken cancellationToken)
        {
            var @event = await _meetingService.ScheduleMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);

            await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

            return new ScheduleMeetingResponse
            {
                Data = @event.Meeting
            };
        }
    }
}