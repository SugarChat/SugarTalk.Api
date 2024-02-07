using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    public Task<List<(MeetingRecord, Meeting)>> GetMeetingRecordsByUserIdAsync(int userId, CancellationToken cancellationToken);
}

public partial class MeetingDataProvider
{
    public async Task<List<(MeetingRecord, Meeting)>> GetMeetingRecordsByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        var historyMeetingIdList = await _repository.QueryNoTracking<MeetingUserSession>()
            .Where(x => x.UserId == userId)
            .Select(x => x.MeetingId)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var completedMeeting = await _repository.QueryNoTracking<Meeting>()
            .Where(x => x.Status == MeetingStatus.Completed && historyMeetingIdList.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x, cancellationToken)
            .ConfigureAwait(false);

        var meetingRecords = await _repository.QueryNoTracking<MeetingRecord>()
            .Where(x => completedMeeting.ContainsKey(x.MeetingId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return meetingRecords.Select(x => (x, completedMeeting.GetValueOrDefault(x.MeetingId))).ToList();
    }
}