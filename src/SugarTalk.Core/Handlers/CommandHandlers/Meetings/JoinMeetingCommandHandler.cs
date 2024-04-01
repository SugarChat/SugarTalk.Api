using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings
{
    [AllowGuestAccess]
    public class JoinMeetingCommandHandler : ICommandHandler<JoinMeetingCommand, JoinMeetingResponse>
    {
        private readonly IMeetingService _meetingService;

        public JoinMeetingCommandHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        public async Task<JoinMeetingResponse> Handle(IReceiveContext<JoinMeetingCommand> context, CancellationToken cancellationToken)
        {
            var @event = await _meetingService.JoinMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);

            await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

            return new JoinMeetingResponse
            {
                Data = new JoinMeetingResponseData
                {
                    UserId = @event.UserId,
                    Meeting = @event.Meeting,
                    MeetingUserSetting = @event.MeetingUserSetting
                }
            };
        }
    }
}