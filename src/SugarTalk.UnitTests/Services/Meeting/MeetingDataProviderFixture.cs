using Shouldly;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Meeting;
using Xunit;

namespace SugarTalk.UnitTests.Services.Meeting;

public class MeetingDataProviderFixture : BaseFixture
{
    [Fact]
    public async Task CanGetMeetingRecord()
    {
        const int userId1 = 1;
        const int userId2 = 2;

        var meetingId1 = Guid.NewGuid();
        var meetingId2 = Guid.NewGuid();
        var meetingId3 = Guid.NewGuid();

        MockUserAccountDb(_repository, new List<UserAccount>
        {
            CreateUserAccountEvent(userId1),
            CreateUserAccountEvent(userId1)
        });

        MockMeetingDb(_repository, new List<Core.Domain.Meeting.Meeting>
        {
            CreateMeetingEvent(
                meetingId1,
                DateTimeOffset.Now.AddDays(-1).ToUnixTimeMilliseconds(),
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Guid.NewGuid().ToString(),
                status: MeetingStatus.Completed
            ),
            CreateMeetingEvent(
                meetingId2,
                DateTimeOffset.Now.AddDays(-1).ToUnixTimeMilliseconds(),
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Guid.NewGuid().ToString(),
                status: MeetingStatus.Pending
            ),
            CreateMeetingEvent(
                meetingId3,
                DateTimeOffset.Now.AddDays(-1).ToUnixTimeMilliseconds(),
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Guid.NewGuid().ToString(),
                status: MeetingStatus.Completed
            )
        });

        MockMeetingUserSessionDb(_repository, new List<MeetingUserSession>
        {
            CreateMeetingUserSessionEvent(1, userId1, meetingId1),
            CreateMeetingUserSessionEvent(2, userId1, meetingId2),
            CreateMeetingUserSessionEvent(3, userId2, meetingId3)
        });

        MockMeetingRecordDb(_repository, new List<MeetingRecord>
        {
            CreateMeetingRecordEvent(Guid.NewGuid(), meetingId1, "", DateTimeOffset.Now),
            CreateMeetingRecordEvent(Guid.NewGuid(), meetingId2, "", DateTimeOffset.Now),
            CreateMeetingRecordEvent(Guid.NewGuid(), meetingId3, "", DateTimeOffset.Now)
        });

        var response = await _meetingDataProvider
            .GetMeetingRecordsByUserIdAsync(userId1, CancellationToken.None);
        response.Count.ShouldBe(1);
        response[0].Item2.Id.ShouldBe(meetingId1);
        response[0].Item1.MeetingId.ShouldBe(meetingId1);
    }
}