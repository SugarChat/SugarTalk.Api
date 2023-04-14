using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Responses;

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
            return await _meetingService.JoinMeeting(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}