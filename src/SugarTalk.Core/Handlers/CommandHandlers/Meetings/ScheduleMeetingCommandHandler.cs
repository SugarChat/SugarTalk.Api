using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dtos.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings
{
    public class ScheduleMeetingCommandHandler: ICommandHandler<ScheduleMeetingCommand, SugarTalkResponse<MeetingDto>>
    {
        private readonly IMeetingService _meetingService;

        public ScheduleMeetingCommandHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }
        
        public async Task<SugarTalkResponse<MeetingDto>> Handle(IReceiveContext<ScheduleMeetingCommand> context, CancellationToken cancellationToken)
        {
            return await _meetingService.ScheduleMeeting(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}