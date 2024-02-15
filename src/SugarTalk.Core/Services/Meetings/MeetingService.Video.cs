using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SugarTalk.Core.Services.Meetings
{
    public partial interface IMeetingService : IScopedDependency
    {
        Task<StorageMeetingVideoDto> StorageMeetingVideoAsync(StorageMeetingVideoByMeetingNumberCommand command, CancellationToken cancellationToken);
    }

    public partial class MeetingService
    {
        public async Task<StorageMeetingVideoDto> StorageMeetingVideoAsync(StorageMeetingVideoByMeetingNumberCommand command, CancellationToken cancellationToken)
        {
            var meeting = await _meetingDataProvider.GetMeetingAsync(command.MeetingNumber, cancellationToken);
        }
    }
}
