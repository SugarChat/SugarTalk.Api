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
    public class StorageMeetingVideoByMeetingNumberCommandHandler : ICommandHandler<StorageMeetingVideoByMeetingNumberCommand, StorageMeetingVideoByMeetingNumberResponse>
    {
        private readonly IMeetingService _meetingService;

        public StorageMeetingVideoByMeetingNumberCommandHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }
        public Task<StorageMeetingVideoByMeetingNumberResponse> Handle(IReceiveContext<StorageMeetingVideoByMeetingNumberCommand> context, CancellationToken cancellationToken)
        {
            _meetingService.StorageMeetingVideoAsync(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}
