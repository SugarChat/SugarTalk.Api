using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings
{
    public class JoinMeetingCommandHandler : ICommandHandler<JoinMeetingCommand, JoinMeetingResponse>
    {
        private readonly IMeetingService _meetingService;

        public JoinMeetingCommandHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        public async Task<JoinMeetingResponse> Handle(IReceiveContext<JoinMeetingCommand> context, CancellationToken cancellationToken)
        {
            return await _meetingService.JoinMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}