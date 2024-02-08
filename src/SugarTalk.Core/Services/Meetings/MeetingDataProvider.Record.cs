using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    public Task<List<MeetingRecordDto>> GetMeetingRecordsByUserIdAsync(GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken);
}

public partial class MeetingDataProvider
{
    public async Task<List<MeetingRecordDto>> GetMeetingRecordsByUserIdAsync(GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.Id;
        if (currentUserId == null)
        {
            return new List<MeetingRecordDto>();
        }


        var joinResult = await _repository.QueryNoTracking<MeetingRecord>()
            .Join(_repository.QueryNoTracking<Meeting>(), record => record.MeetingId, meeting => meeting.Id,
                (record, meeting) => new
                {
                    Record = record,
                    Meeting = meeting
                })
            .Join(_repository.QueryNoTracking<MeetingUserSession>(), result => result.Meeting.Id, session => session.MeetingId,
                (result, session) => new
                {
                    result.Meeting,
                    result.Record,
                    Session = session
                })
            .Join(_repository.QueryNoTracking<UserAccount>(), result => result.Meeting.MeetingMasterUserId, user => user.Id,
                (result, user) => new
                {
                    result.Meeting,
                    result.Record,
                    result.Session,
                    User = user
                })
            .Where(x => x.Session.UserId.Equals(currentUserId))
            .OrderByDescending(x => x.Record.CreatedDate)
            .Skip((request.PageSetting.Page - 1) * request.PageSetting.PageSize)
            .Take(request.PageSetting.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return joinResult.Select(x => new MeetingRecordDto
            {
                MeetingId = x.Meeting.Id,
                MeetingNumber = x.Meeting.MeetingNumber,
                Title = x.Meeting.Title,
                StartDate = x.Meeting.StartDate,
                EndDate = x.Meeting.EndDate,
                Timezone = x.Meeting.TimeZone,
                MeetingCreator = x.User.UserName
            })
            .ToList();
    }
}