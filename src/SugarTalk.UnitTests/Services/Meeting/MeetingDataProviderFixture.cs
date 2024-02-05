using Xunit;
using Shouldly;
using NSubstitute;
using SugarTalk.Messages.Dto;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.UnitTests.Services.Meeting;

public class MeetingDataProviderFixture : BaseFixture
{
    [Fact]
    public async Task CanGetMeetingHistories()
    {
        const int user1Id = 1;
        const int user2Id = 2;
        var oneHour = 3600;
        
        var meeting1Id = Guid.NewGuid();
        var meeting2Id = Guid.NewGuid();
        var meeting3Id = Guid.NewGuid();
        var meeting4Id = Guid.NewGuid();
        var meeting5Id = Guid.NewGuid();

        _clock.Now.Returns(new DateTimeOffset(2024, 2, 1, 7, 0, 0, TimeSpan.Zero));

        MockUserAccountsDb(_repository, new List<UserAccount>
        {
            CreateUserAccountEvent(user1Id, "test1"),
            CreateUserAccountEvent(user2Id, "test2")
        });

        MockMeetingDb(_repository, new List<Core.Domain.Meeting.Meeting>
        {
            CreateMeetingEvent(meeting1Id, meetingNumber: "123456",
                startDate: _clock.Now.AddDays(-1).ToUnixTimeSeconds(), endDate: _clock.Now.AddDays(-1).AddHours(1).ToUnixTimeSeconds(), status: MeetingStatus.Completed),
            CreateMeetingEvent(meeting2Id, meetingNumber: "111222",
                startDate: _clock.Now.AddDays(-1).AddHours(1).ToUnixTimeSeconds(), endDate: _clock.Now.AddDays(-1).AddHours(2).ToUnixTimeSeconds(), status: MeetingStatus.Completed),
            CreateMeetingEvent(meeting3Id, meetingNumber: "111222",
                startDate: _clock.Now.AddDays(-1).AddHours(2).ToUnixTimeSeconds(), endDate: _clock.Now.AddDays(-1).AddHours(3).ToUnixTimeSeconds(), status: MeetingStatus.Completed),
            CreateMeetingEvent(meeting4Id, meetingNumber: "111222",
                startDate: _clock.Now.AddDays(-1).AddHours(3).ToUnixTimeSeconds(), endDate: _clock.Now.AddDays(-1).AddHours(4).ToUnixTimeSeconds(), status: MeetingStatus.Completed),
            CreateMeetingEvent(meeting5Id, meetingNumber: "222333",
                startDate: _clock.Now.ToUnixTimeSeconds(), endDate: _clock.Now.AddHours(1).ToUnixTimeSeconds(), status: MeetingStatus.Pending)
        });
        
        MockUserSessionDb(_repository, new List<MeetingUserSession>
        {
            CreateUserSessionEvent(1, user1Id, meeting1Id),
            CreateUserSessionEvent(2, user1Id, meeting2Id),
            CreateUserSessionEvent(3, user1Id, meeting3Id),
            CreateUserSessionEvent(4, user1Id, meeting4Id),
            CreateUserSessionEvent(5, user2Id, meeting4Id),
            CreateUserSessionEvent(6, user1Id, meeting5Id)
        });

        var response = await _meetingDataProvider
            .GetMeetingHistoriesByUserIdAsync(user1Id, new PageSetting { Page = 1, PageSize = 2 }, CancellationToken.None);
        
        response.TotalCount.ShouldBe(4);
        
        response.MeetingHistoryList.Count.ShouldBe(2);
        
        response.MeetingHistoryList.First().MeetingId.ShouldBe(meeting4Id);
        response.MeetingHistoryList.First().attendees.Count.ShouldBe(2);
        response.MeetingHistoryList.First().MeetingCreator.ShouldBe("test1");
        response.MeetingHistoryList.First().Duration.ShouldBe(oneHour);
        
        response.MeetingHistoryList.Last().MeetingId.ShouldBe(meeting3Id);
        response.MeetingHistoryList.Last().attendees.Count.ShouldBe(1);
        response.MeetingHistoryList.Last().MeetingCreator.ShouldBe("test1");
        response.MeetingHistoryList.Last().Duration.ShouldBe(oneHour);
    }
}