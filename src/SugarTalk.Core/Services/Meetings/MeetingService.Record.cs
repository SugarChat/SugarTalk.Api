using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    public Task<GetMeetingRecordByUserResponse> GetMeetingRecordsByUserIdAsync(GetMeetingRecordByUserRequest request,
        CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<GetMeetingRecordByUserResponse> GetMeetingRecordsByUserIdAsync(GetMeetingRecordByUserRequest request, CancellationToken cancellationToken)
    {
        if (_currentUser.Id == null)
        {
            return new GetMeetingRecordByUserResponse
            {
                MeetingRecordList = new List<MeetingRecordDto>()
            };
        }

        var meetingRecords = await _meetingDataProvider.GetMeetingRecordsByUserIdAsync(_currentUser.Id.Value, cancellationToken)
            .ConfigureAwait(false);

        var users = await _accountDataProvider
            .GetUserAccountsAsync(meetingRecords.Select(x => x.Item2.MeetingMasterUserId).ToList(), cancellationToken)
            .ConfigureAwait(false);

        var response = new GetMeetingRecordByUserResponse
        {
            MeetingRecordList = meetingRecords.Select(x => new MeetingRecordDto
            {
                MeetingId = x.Item2.Id,
                MeetingNumber = x.Item2.MeetingNumber,
                Title = x.Item2.Title,
                StartDate = x.Item2.StartDate,
                EndDate = x.Item2.EndDate,
                Duration = x.Item2.StartDate == 0 || x.Item2.EndDate == 0 || x.Item2.EndDate <= x.Item2.StartDate
                    ? x.Item2.EndDate - x.Item2.StartDate
                    : 0,
                Timezone = x.Item2.TimeZone,
                MeetingCreator = users.FirstOrDefault(u => x.Item2.MeetingMasterUserId == u.Id)?.UserName
            }).ToList()
        };
        return response;
    }
}