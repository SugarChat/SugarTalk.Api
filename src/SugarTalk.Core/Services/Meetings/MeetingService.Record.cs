using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Exceptions;

namespace SugarTalk.Core.Services.Meetings
{
    public partial interface IMeetingService : IScopedDependency
    {
        Task<StorageMeetingRecordVideoResponse> StorageMeetingRecordVideoAsync(StorageMeetingRecordVideoCommand command, CancellationToken cancellationToken);
    }

    public partial class MeetingService
    {
        public async Task<StorageMeetingRecordVideoResponse> StorageMeetingRecordVideoAsync(StorageMeetingRecordVideoCommand command, CancellationToken cancellationToken)
        {
            var meeting=  await _meetingDataProvider.GetMeetingByIdAsync(command.MeetingId,cancellationToken).ConfigureAwait(false);
            if (meeting == null) throw new MeetingNotFoundException();
            var count = await (await _meetingDataProvider.GetMeetingRecordsByMeetingIdAsync(command.MeetingId, cancellationToken).ConfigureAwait(false))
                .CountAsync(cancellationToken);
            var meetingRecord = _mapper.Map<MeetingRecord>(meeting);
            meetingRecord.Url = command.Url;
            var recordNumber = 1 + count;
            string stringNumber =  recordNumber.ToString().PadLeft(6, '0');
            meetingRecord.RecordNumber = "ZNZX" + meeting.CreatedDate.ToString("yyyMMdd") + stringNumber;
            await _meetingDataProvider.StorageMeetingRecord(meetingRecord, cancellationToken).ConfigureAwait(false);
            return new StorageMeetingRecordVideoResponse();
        }
    }
}
