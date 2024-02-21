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
    public class StorageMeetingRecordVideoCommandHandler : ICommandHandler<StorageMeetingRecordVideoCommand, StorageMeetingRecordVideoResponse>
    {
        private readonly IMeetingService _meetingService;
        
        public StorageMeetingRecordVideoCommandHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }
        
        public async Task<StorageMeetingRecordVideoResponse> Handle(IReceiveContext<StorageMeetingRecordVideoCommand> context, CancellationToken cancellationToken)
        {
           var response = await _meetingService.StorageMeetingRecordVideoAsync(context.Message, cancellationToken).ConfigureAwait(false);
           
           return response;
        }
    }
}
