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
    public class VerifyMeetingUserPermissionCommandHandler : ICommandHandler<VerifyMeetingUserPermissionCommand, VerifyMeetingUserPermissionResponse>
    {
        private readonly IMeetingService _meetingService;

        public VerifyMeetingUserPermissionCommandHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }
        
        public async Task<VerifyMeetingUserPermissionResponse> Handle(IReceiveContext<VerifyMeetingUserPermissionCommand> context, CancellationToken cancellationToken)
        {
            return await _meetingService.VerifyMeetingUserPermissionAsync(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}
