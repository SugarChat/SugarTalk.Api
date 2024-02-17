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
using Newtonsoft.Json;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Messages.Enums.Meeting;

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
            var meetingRecord = await _meetingDataProvider.GetMeetingRecordByMeetingIdAsync(command.MeetingId, cancellationToken);
            if (meetingRecord == null) throw new MeetingRecordNotFoundException();
            if (meetingRecord.RecordType == MeetingRecordType.EndRecord) throw new MeetingRecordNotOpenException();
            
            var response = await _liveKitClient.StopEgressAsync(new StopEgressRequestDto { EgressId = meetingRecord.EgressId },cancellationToken).ConfigureAwait(false);
            dynamic data = JsonConvert.DeserializeObject(response);
            meetingRecord.RecordType = MeetingRecordType.EndRecord;
            meetingRecord.Url = data["file"]["location"];
            
            await _meetingDataProvider.UpdateMeetingRecordAsync(meetingRecord, cancellationToken).ConfigureAwait(false);
            return new StorageMeetingRecordVideoResponse();
        }
    }
}
