using Xunit;
using Shouldly;
using NSubstitute;
using SugarTalk.Messages.Dto;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

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

        MockUserAccountsDb(_repository, new List<UserAccount>
        {
            CreateUserAccountEvent(userId1, Guid.NewGuid()),
            CreateUserAccountEvent(userId2, Guid.NewGuid())
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

        MockUserSessionDb(_repository, new List<MeetingUserSession>
        {
            CreateUserSessionEvent(1, userId1, meetingId1),
            CreateUserSessionEvent(2, userId2, meetingId2),
            CreateUserSessionEvent(3, userId2, meetingId3)
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
        var (count, items) = await _meetingDataProvider.GetMeetingRecordsByUserIdAsync(1, getCurrentUserMeetingRecordRequest, CancellationToken.None);
        count.ShouldBe(1);
        items[0].MeetingId.ShouldBe(meetingId1);
    }

    [Fact]
    public async Task JoinMeetingButMeetingNotFinishCase()
    {
        const int userId1 = 1;
        const int userId2 = 2;

        var meetingId1 = Guid.NewGuid();
        var meetingId2 = Guid.NewGuid();
        var meetingId3 = Guid.NewGuid();

        MockUserAccountsDb(_repository, new List<UserAccount>
        {
            CreateUserAccountEvent(userId1, Guid.NewGuid()),
            CreateUserAccountEvent(userId2, Guid.NewGuid())
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

        MockUserSessionDb(_repository, new List<MeetingUserSession>
        {
            CreateUserSessionEvent(1, userId1, meetingId1),
            CreateUserSessionEvent(2, userId2, meetingId2),
            CreateUserSessionEvent(3, userId2, meetingId3)
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
        var (count, items) = await _meetingDataProvider.GetMeetingRecordsByUserIdAsync(1, getCurrentUserMeetingRecordRequest, CancellationToken.None);
        count.ShouldBe(0);
    }

    [Fact]
    public async Task CanDeleteMeetingHistory()
    {
        var meetingHistory1Id = Guid.NewGuid();
        var meetingHistory2Id = Guid.NewGuid();
        var meetingHistory3Id = Guid.NewGuid();
        
        var meeting1Id = Guid.NewGuid();
        var meeting2Id = Guid.NewGuid();
        var meeting3Id = Guid.NewGuid();
        
        MockMeetingHistoriesDb(_repository, new List<MeetingHistory>
        {
            CreateMeetingHistoryEvent(meetingHistory1Id, meeting1Id, userId: 1, null),
            CreateMeetingHistoryEvent(meetingHistory2Id, meeting2Id, userId: 1, null),
            CreateMeetingHistoryEvent(meetingHistory3Id, meeting3Id, userId: 1, null)
        });

        await _meetingDataProvider.DeleteMeetingHistoryAsync(new List<Guid> { meetingHistory1Id, meetingHistory2Id }, 1, CancellationToken.None);

        var meetingHistories = await _repository
            .Query<MeetingHistory>().Where(x => x.UserId == 1 && !x.IsDeleted).ToListAsync();

        meetingHistories.Count(x =>
            x.MeetingId == meeting3Id &&
            x.Id == meetingHistory3Id).ShouldBe(1);
    }
    
    [Fact]
    public async Task CanDeleteMeetingRecord()
    {
        var meetingRecord1Id = Guid.NewGuid();
        var meetingRecord2Id = Guid.NewGuid();
        var meetingRecord3Id = Guid.NewGuid();
        
        var meeting1Id = Guid.NewGuid();
        var meeting2Id = Guid.NewGuid();
        var meeting3Id = Guid.NewGuid();
        
        MockMeetingRecordDb(_repository, new List<MeetingRecord>
        {
            CreateMeetingRecordEvent(meetingRecord1Id, meeting1Id, "", _clock.Now),
            CreateMeetingRecordEvent(meetingRecord2Id, meeting2Id, "", _clock.Now),
            CreateMeetingRecordEvent(meetingRecord3Id, meeting3Id, "", _clock.Now)
        });

        await _meetingDataProvider.DeleteMeetingRecordAsync(new List<Guid> { meetingRecord1Id, meetingRecord2Id }, CancellationToken.None);

        var meetingRecords = await _repository
            .Query<MeetingRecord>().Where(x => !x.IsDeleted).ToListAsync();

        meetingRecords.Count(x =>
            x.MeetingId == meeting3Id &&
            x.Id == meetingRecord3Id).ShouldBe(1);
    }

    [Fact]
    public async Task CanUpdateMeetingStatusWhenOutMeeting()
    {
        const int userId1 = 1;
        const int userId2 = 2;
        const int userId3 = 3;
        
        var meetingId1 = Guid.NewGuid();
        var meetingId2 = Guid.NewGuid();
        var meetingId3 = Guid.NewGuid();
        
        var meetingSubId1 = Guid.NewGuid();
        var meetingSubId2 = Guid.NewGuid();

        _clock.Now.Returns(DateTimeOffset.Now);

        MockMeetingDb(_repository, new List<Core.Domain.Meeting.Meeting>
        {
            CreateMeetingEvent(
                meetingId1,
                appointmentType: MeetingAppointmentType.Appointment,
                status: MeetingStatus.InProgress,
                startDate: _clock.Now.AddHours(1).ToUnixTimeSeconds(),
                endDate: _clock.Now.AddHours(2).ToUnixTimeSeconds(),
                meetingNumber: Guid.NewGuid().ToString()
            ),
            CreateMeetingEvent(
                meetingId2,
                appointmentType: MeetingAppointmentType.Appointment,
                status: MeetingStatus.InProgress,
                startDate: _clock.Now.AddHours(-2).ToUnixTimeSeconds(),
                endDate: _clock.Now.AddHours(-1).ToUnixTimeSeconds(),
                meetingNumber: Guid.NewGuid().ToString()
            ),
            CreateMeetingEvent(
                meetingId3,
                appointmentType: MeetingAppointmentType.Quick,
                status: MeetingStatus.InProgress,
                startDate: _clock.Now.ToUnixTimeSeconds(),
                endDate: _clock.Now.AddHours(1).ToUnixTimeSeconds(),
                meetingNumber: Guid.NewGuid().ToString()
            )
        });

        MockUserSessionDb(_repository, new List<MeetingUserSession>
        {
            CreateUserSessionEvent(1, userId1, meetingId1, meetingSubId1, status: MeetingAttendeeStatus.Present),
            CreateUserSessionEvent(2, userId2, meetingId1, meetingSubId1, status: MeetingAttendeeStatus.Absent),
            CreateUserSessionEvent(3, userId3, meetingId1, meetingSubId2, status: MeetingAttendeeStatus.Present),
            
            CreateUserSessionEvent(4, userId1, meetingId2, status: MeetingAttendeeStatus.Present),
            CreateUserSessionEvent(5, userId2, meetingId2, status: MeetingAttendeeStatus.Absent),
            CreateUserSessionEvent(6, userId3, meetingId2, status: MeetingAttendeeStatus.Absent),
            
            CreateUserSessionEvent(7, userId1, meetingId3, meetingSubId1, status: MeetingAttendeeStatus.Absent)
        });
        
        await _meetingService.HandleMeetingStatusWhenOutMeetingAsync(userId1, meetingId1, meetingSubId1, CancellationToken.None);
        var response1 = await _repository.Query<Core.Domain.Meeting.Meeting>().FirstOrDefaultAsync(x => x.Id == meetingId1);
        response1?.Status.ShouldBe(MeetingStatus.Pending);
        
        await _meetingService.HandleMeetingStatusWhenOutMeetingAsync(userId1, meetingId2, null, CancellationToken.None);
        var response2 = await _repository.Query<Core.Domain.Meeting.Meeting>().FirstOrDefaultAsync(x => x.Id == meetingId2);
        response2?.Status.ShouldBe(MeetingStatus.Completed);

        await _meetingService.HandleMeetingStatusWhenOutMeetingAsync(userId1, meetingId3, null, CancellationToken.None);
        var response3 = await _repository.Query<Core.Domain.Meeting.Meeting>().FirstOrDefaultAsync(x => x.Id == meetingId3);
        response3?.Status.ShouldBe(MeetingStatus.Completed);
    }

    [Fact]
    public async Task CanGetMeetingHistoryByKeyword()
    {
        const int userId1 = 1;
        const int userId2 = 2;
        
        var meetingHistory1Id = Guid.NewGuid();
        var meetingHistory2Id = Guid.NewGuid();
        var meetingHistory3Id = Guid.NewGuid();
        
        var meeting1Id = Guid.NewGuid();
        var meeting2Id = Guid.NewGuid();
        var meeting3Id = Guid.NewGuid();
        
        MockMeetingDb(_repository, new List<Core.Domain.Meeting.Meeting>
        {
            CreateMeetingEvent(meeting1Id, meetingMasterUserId: userId1, meetingNumber: "123456", title: "mars meeting1"),
            CreateMeetingEvent(meeting2Id, meetingMasterUserId: userId1, meetingNumber: "111111", title: "mars meeting2"),
            CreateMeetingEvent(meeting3Id, meetingMasterUserId: userId2, meetingNumber: "666666", title: "greg meeting")
        });

        MockMeetingHistoriesDb(_repository, new List<MeetingHistory>
        {
            CreateMeetingHistoryEvent(meetingHistory1Id, meeting1Id, userId: userId1, null),
            CreateMeetingHistoryEvent(meetingHistory2Id, meeting2Id, userId: userId1, null),
            CreateMeetingHistoryEvent(meetingHistory3Id, meeting3Id, userId: userId1, null)
        });
        
        MockUserSessionDb(_repository, new List<MeetingUserSession>
        {
            CreateUserSessionEvent(1, userId1, meeting1Id),
            CreateUserSessionEvent(2, userId2, meeting2Id),
            CreateUserSessionEvent(3, userId2, meeting3Id)
        });
        
        MockUserAccountsDb(_repository, new List<UserAccount>
        {
           CreateUserAccountEvent(userId: userId1, Guid.NewGuid(), "mars"),
           CreateUserAccountEvent(userId: userId2, Guid.NewGuid(), "greg")
        });

        var response1 = await _meetingDataProvider
            .GetMeetingHistoriesByUserIdAsync(1, "greg", null, CancellationToken.None);
        
        response1.TotalCount.ShouldBe(1);
        response1.MeetingHistoryList.Count(x => x.MeetingId == meeting3Id && x.MeetingCreator == "greg").ShouldBe(1);
        
        var response2 = await _meetingDataProvider
            .GetMeetingHistoriesByUserIdAsync(1, "123456", null, CancellationToken.None);
        
        response2.TotalCount.ShouldBe(1);
        response2.MeetingHistoryList.Count(x => x.MeetingId == meeting1Id && x.MeetingCreator == "mars").ShouldBe(1);
        
        var response3 = await _meetingDataProvider
            .GetMeetingHistoriesByUserIdAsync(1, "mars meeting", null, CancellationToken.None);
        
        response3.TotalCount.ShouldBe(2);
        response3.MeetingHistoryList.Count(x => x.MeetingId == meeting2Id && x.MeetingCreator == "mars").ShouldBe(1);
        
        var response4 = await _meetingDataProvider
            .GetMeetingHistoriesByUserIdAsync(1, "greg777999", null, CancellationToken.None);
        
        response4.TotalCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("mock url")]
    [InlineData("mock url1")]
    public async Task CanGetMeetingRecordByMeetingRecordId(string mockUrl)
    {
        var meetingRecordId = new Guid();
        var meetingRecordId1 = new Guid();

        MockMeetingRecordDb(_repository, new List<MeetingRecord>
        {
            CreateMeetingRecordEvent(meetingRecordId, new Guid(), mockUrl, DateTimeOffset.Now),
        });
        
        var meetingRecord = await _meetingDataProvider.GetMeetingRecordByMeetingRecordIdAsync(meetingRecordId, CancellationToken.None);
        
        meetingRecord.Id.ShouldBe(meetingRecordId);
        meetingRecord.RecordType.ShouldBe(MeetingRecordType.OnRecord);
    }
}