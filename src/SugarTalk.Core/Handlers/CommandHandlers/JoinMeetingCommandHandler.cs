using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands;
using SugarTalk.Messages.Dtos.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers
{
    public class JoinMeetingCommandHandler : ICommandHandler<JoinMeetingCommand, SugarTalkResponse<MeetingSessionDto>>
    {
        private readonly IMeetingService _meetingService;

        public JoinMeetingCommandHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        public async Task<SugarTalkResponse<MeetingSessionDto>> Handle(IReceiveContext<JoinMeetingCommand> context, CancellationToken cancellationToken)
        {
            return await _meetingService.JoinMeeting(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}