using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<List<MeetingRecord>> GetMeetingRecordsAsync(Guid? id = null, CancellationToken cancellationToken = default);
}

public partial class MeetingDataProvider
{
    public async Task<List<MeetingRecord>> GetMeetingRecordsAsync(
        Guid? id = null, CancellationToken cancellationToken = default)
    {
        var query = _repository.Query<MeetingRecord>();
        
        if (id.HasValue)
            query = query.Where(x => x.Id == id.Value);
        
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}