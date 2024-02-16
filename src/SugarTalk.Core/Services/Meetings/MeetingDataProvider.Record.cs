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
    Task StorageMeetingRecord(MeetingRecord record, CancellationToken cancellationToken);
    Task<IQueryable<MeetingRecord>> GetMeetingRecordsByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken);
}

public partial class MeetingDataProvider
{
    public async Task StorageMeetingRecord(MeetingRecord record,CancellationToken cancellationToken)
    {
        if (record == null) return;
        
        await _repository.InsertAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IQueryable<MeetingRecord>> GetMeetingRecordsByMeetingIdAsync(Guid meetingId,CancellationToken cancellationToken)
    {
        var meetingRecords=  _repository.QueryNoTracking<MeetingRecord>(x => x.MeetingId == meetingId);
        return meetingRecords;
    }
}