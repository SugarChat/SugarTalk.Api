using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task UpdateMeetingRecordAsync(MeetingRecord record, CancellationToken cancellationToken);
    
    Task<MeetingRecord> GetMeetingRecordByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken);
}

public partial class MeetingDataProvider
{
    public async Task UpdateMeetingRecordAsync(MeetingRecord record,CancellationToken cancellationToken)
    {
        if (record == null) return;
        
        await _repository.UpdateAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MeetingRecord> GetMeetingRecordByMeetingIdAsync(Guid meetingId,CancellationToken cancellationToken)
    {
        var meetingRecord = await _repository.QueryNoTracking<MeetingRecord>(x => x.MeetingId == meetingId)
            .OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return meetingRecord;
    }
}