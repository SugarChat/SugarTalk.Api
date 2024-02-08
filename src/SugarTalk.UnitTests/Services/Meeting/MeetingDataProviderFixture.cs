using Shouldly;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto;
using SugarTalk.Messages.Requests.Meetings;
using Xunit;

namespace SugarTalk.UnitTests.Services.Meeting;

public class MeetingDataProviderFixture : BaseFixture
{
    [Fact]
    public async Task CanGetMeetingRecordCase()
    {
        const int userId1 = 1;
        const int userId2 = 2;

        var meetingId1 = Guid.NewGuid();
        var meetingId2 = Guid.NewGuid();
        var meetingId3 = Guid.NewGuid();

        MockUserAccountDb(_repository, new List<UserAccount>
        {
            CreateUserAccountEvent(userId1),
            CreateUserAccountEvent(userId2)
        });

        MockMeetingDb(_repository, new List<Core.Domain.Meeting.Meeting>
        {
            CreateMeetingEvent(
                meetingId1,
                DateTimeOffset.Now.AddDays(-1).ToUnixTimeMilliseconds(),
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Guid.NewGuid().ToString(),
                meetingMasterUserId: userId1
            ),
            CreateMeetingEvent(
                meetingId2,
                DateTimeOffset.Now.AddDays(-1).ToUnixTimeMilliseconds(),
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Guid.NewGuid().ToString(),
                meetingMasterUserId: userId2
            ),
            CreateMeetingEvent(
                meetingId3,
                DateTimeOffset.Now.AddDays(-1).ToUnixTimeMilliseconds(),
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Guid.NewGuid().ToString(),
                meetingMasterUserId: userId2
            )
        });

        MockMeetingUserSessionDb(_repository, new List<MeetingUserSession>
        {
            CreateMeetingUserSessionEvent(1, userId1, meetingId1),
            CreateMeetingUserSessionEvent(2, userId2, meetingId2),
            CreateMeetingUserSessionEvent(3, userId2, meetingId3)
        });

        MockMeetingRecordDb(_repository, new List<MeetingRecord>
        {
            CreateMeetingRecordEvent(Guid.NewGuid(), meetingId1, "", DateTimeOffset.Now),
            CreateMeetingRecordEvent(Guid.NewGuid(), meetingId2, "", DateTimeOffset.Now),
            CreateMeetingRecordEvent(Guid.NewGuid(), meetingId3, "", DateTimeOffset.Now)
        });

        var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
        {
            PageSetting = new PageSetting
            {
                PageSize = 100,
                Page = 1
            }
        };
        var response = await _meetingDataProvider.GetMeetingRecordsByUserIdAsync(getCurrentUserMeetingRecordRequest, CancellationToken.None);
        response.Count.ShouldBe(1);
        response[0].MeetingId.ShouldBe(meetingId1);
    }

    [Fact]
    public async Task JoinMeetingButMeetingNotFinishCase()
    {
        const int userId1 = 1;
        const int userId2 = 2;

        var meetingId1 = Guid.NewGuid();
        var meetingId2 = Guid.NewGuid();
        var meetingId3 = Guid.NewGuid();

        MockUserAccountDb(_repository, new List<UserAccount>
        {
            CreateUserAccountEvent(userId1),
            CreateUserAccountEvent(userId2)
        });

        MockMeetingDb(_repository, new List<Core.Domain.Meeting.Meeting>
        {
            CreateMeetingEvent(
                meetingId1,
                DateTimeOffset.Now.AddDays(-1).ToUnixTimeMilliseconds(),
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Guid.NewGuid().ToString(),
                meetingMasterUserId: userId1
            ),
            CreateMeetingEvent(
                meetingId2,
                DateTimeOffset.Now.AddDays(-1).ToUnixTimeMilliseconds(),
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Guid.NewGuid().ToString(),
                meetingMasterUserId: userId2
            ),
            CreateMeetingEvent(
                meetingId3,
                DateTimeOffset.Now.AddDays(-1).ToUnixTimeMilliseconds(),
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Guid.NewGuid().ToString(),
                meetingMasterUserId: userId2
            )
        });

        MockMeetingUserSessionDb(_repository, new List<MeetingUserSession>
        {
            CreateMeetingUserSessionEvent(1, userId1, meetingId1),
            CreateMeetingUserSessionEvent(2, userId2, meetingId2),
            CreateMeetingUserSessionEvent(3, userId2, meetingId3)
        });

        MockMeetingRecordDb(_repository, new List<MeetingRecord>
        {
            CreateMeetingRecordEvent(Guid.NewGuid(), meetingId2, "", DateTimeOffset.Now),
            CreateMeetingRecordEvent(Guid.NewGuid(), meetingId3, "", DateTimeOffset.Now)
        });

        var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
        {
            PageSetting = new PageSetting
            {
                PageSize = 100,
                Page = 1
            }
        };
        var response = await _meetingDataProvider.GetMeetingRecordsByUserIdAsync(getCurrentUserMeetingRecordRequest, CancellationToken.None);
        response.Count.ShouldBe(0);
    }
}