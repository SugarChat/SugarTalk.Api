using Microsoft.EntityFrameworkCore;
using Shouldly;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto;
using SugarTalk.Messages.Enums.Meeting;
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
    public async Task CanGetMeetingRecordByMeetingRecordId()
    {
        var meetingRecordId = new Guid();
        var meetingRecordId1 = new Guid();

        MockMeetingRecordDb(_repository, new List<MeetingRecord>
        {
            CreateMeetingRecordEvent(meetingRecordId, new Guid(), "mock url", DateTimeOffset.Now),
            CreateMeetingRecordEvent(meetingRecordId1, new Guid(), "mock url1", DateTimeOffset.Now)
        });
        var meetingRecord =
            await _meetingDataProvider.GetMeetingRecordByMeetingRecordIdAsync(meetingRecordId, CancellationToken.None);
        var meetingRecord1 =
            await _meetingDataProvider.GetMeetingRecordByMeetingRecordIdAsync(meetingRecordId1, CancellationToken.None);
        meetingRecord.Id.ShouldBe(meetingRecordId);
        meetingRecord.RecordType.ShouldBe(MeetingRecordType.OnRecord);
        meetingRecord1.Id.ShouldBe(meetingRecordId1);
        meetingRecord1.RecordType.ShouldBe(MeetingRecordType.OnRecord);
    }
}