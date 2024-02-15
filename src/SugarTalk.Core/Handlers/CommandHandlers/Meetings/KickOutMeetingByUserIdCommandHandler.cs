using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings
{

    public class KickOutMeetingByUserIdCommandHandler : ICommandHandler<KickOutMeetingByUserIdCommand, KickOutMeetingByUserIdResponse>
    {
        private readonly IMeetingService _meetingService;

        public KickOutMeetingByUserIdCommandHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }
        public async Task<KickOutMeetingByUserIdResponse> Handle(IReceiveContext<KickOutMeetingByUserIdCommand> context, CancellationToken cancellationToken)
        {
            return await _meetingService.KickOutMeetingAsync(context.Message, cancellationToken);
        }
    }

}
