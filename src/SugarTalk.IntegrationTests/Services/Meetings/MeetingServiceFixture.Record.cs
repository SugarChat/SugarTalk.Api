using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Dto;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Fact]
    public async Task CanGetMeetingRecord()
    {
        var testCurrentUser = new TestCurrentUser();
        var otherUser = await _accountUtil.AddUserAccount("user1", "123456").ConfigureAwait(false);

        var meeting1 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "1",
            MeetingMasterUserId = testCurrentUser.Id??-1
        };

        var meeting2 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "2",
            MeetingMasterUserId = otherUser.Id
        };

        var meeting3 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "3",
            MeetingMasterUserId = otherUser.Id
        };

        var meetingRecord1 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1.Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-2),
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url1"
        };

        var meetingRecord2 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1.Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-1),
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url2"
        };

        var meetingRecord3 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting2.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url3"
        };

        var meetingRecord4 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting3.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url4"
        };

        await _meetingUtil.AddMeeting(meeting1).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting2).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting3).ConfigureAwait(false);

        await _meetingUtil.AddMeetingUserSession(1, meeting1.Id, 1);
        await _meetingUtil.AddMeetingUserSession(2, meeting2.Id, otherUser.Id);
        await _meetingUtil.AddMeetingUserSession(3, meeting3.Id, otherUser.Id);

        await _meetingUtil.AddMeetingRecord(meetingRecord1);
        await _meetingUtil.AddMeetingRecord(meetingRecord2);
        await _meetingUtil.AddMeetingRecord(meetingRecord3);
        await _meetingUtil.AddMeetingRecord(meetingRecord4);

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(2);
            var meetingRecordDto = response.Data.Records[0];
            meetingRecordDto.MeetingId.ShouldBe(meeting1.Id);
            meetingRecordDto.MeetingNumber.ShouldBe(meeting1.MeetingNumber);
            meetingRecordDto.MeetingCreator.ShouldBe(testCurrentUser.UserName);
        });
    }

    [Fact]
    public async Task CanGetMeetingRecordByKeyWord()
    {
        var testCurrentUser = new TestCurrentUser();
        var otherUser = await _accountUtil.AddUserAccount("user1", "123456").ConfigureAwait(false);

        var meeting1 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "000111000",
            Title = "111會議",
            MeetingMasterUserId = testCurrentUser.Id??-1
        };

        var meeting2 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "000222000",
            Title = "222會議",
            MeetingMasterUserId = otherUser.Id
        };

        var meeting3 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "000333000",
            Title = "333會議",
            MeetingMasterUserId = otherUser.Id
        };

        var meetingRecord1 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1.Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-2),
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url1"
        };

        var meetingRecord2 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1.Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-1),
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url2"
        };

        var meetingRecord3 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting2.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url3"
        };

        var meetingRecord4 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting3.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url4"
        };

        await _meetingUtil.AddMeeting(meeting1).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting2).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting3).ConfigureAwait(false);

        await _meetingUtil.AddMeetingUserSession(1, meeting1.Id, 1);
        await _meetingUtil.AddMeetingUserSession(2, meeting2.Id, 1);
        await _meetingUtil.AddMeetingUserSession(3, meeting3.Id, otherUser.Id);

        await _meetingUtil.AddMeetingRecord(meetingRecord1);
        await _meetingUtil.AddMeetingRecord(meetingRecord2);
        await _meetingUtil.AddMeetingRecord(meetingRecord3);
        await _meetingUtil.AddMeetingRecord(meetingRecord4);

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                Keyword = "user1",
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(1);
            var meetingRecordDto = response.Data.Records[0];
            meetingRecordDto.MeetingId.ShouldBe(meeting2.Id);
        });

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                Keyword = "0111",
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(2);
            var meetingRecordDto = response.Data.Records[0];
            meetingRecordDto.MeetingId.ShouldBe(meeting1.Id);
        });

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                Keyword = "222會議",
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(1);
            var meetingRecordDto = response.Data.Records[0];
            meetingRecordDto.MeetingId.ShouldBe(meeting2.Id);
        });
    }

    [Fact]
    public async Task ShouldNoRecord()
    {
        var testCurrentUser = new TestCurrentUser();
        var otherUser = await _accountUtil.AddUserAccount("user1", "123456").ConfigureAwait(false);

        var meeting1 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "1",
            MeetingMasterUserId = testCurrentUser.Id??-1
        };

        var meeting2 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "2",
            MeetingMasterUserId = otherUser.Id
        };

        var meeting3 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "3",
            MeetingMasterUserId = otherUser.Id
        };

        var meetingRecord1 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting2.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url3"
        };

        var meetingRecord2 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting3.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url4"
        };

        await _meetingUtil.AddMeeting(meeting1).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting2).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting3).ConfigureAwait(false);

        await _meetingUtil.AddMeetingUserSession(1, meeting1.Id, 1);
        await _meetingUtil.AddMeetingUserSession(2, meeting2.Id, otherUser.Id);
        await _meetingUtil.AddMeetingUserSession(3, meeting3.Id, otherUser.Id);

        await _meetingUtil.AddMeetingRecord(meetingRecord1);
        await _meetingUtil.AddMeetingRecord(meetingRecord2);

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task ShouldMeetingCreatorIsDifferent()
    {
        var otherUser = await _accountUtil.AddUserAccount("user1", "123456").ConfigureAwait(false);
        var testCurrentUser = new TestCurrentUser();
        var meeting1 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "1",
            MeetingMasterUserId = testCurrentUser.Id ?? -1
        };

        var meeting2 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "2",
            MeetingMasterUserId = otherUser.Id
        };

        var meeting3 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "3",
            MeetingMasterUserId = otherUser.Id
        };

        var meetingRecord1 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1.Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-2),
            Url = "mock url1"
        };

        var meetingRecord2 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting2.Id,
            Url = "mock url3"
        };

        var meetingRecord3 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting3.Id,
            Url = "mock url4"
        };

        await _meetingUtil.AddMeeting(meeting1).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting2).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting3).ConfigureAwait(false);

        await _meetingUtil.AddMeetingUserSession(1, meeting1.Id, 1);
        await _meetingUtil.AddMeetingUserSession(2, meeting2.Id, 1);
        await _meetingUtil.AddMeetingUserSession(3, meeting3.Id, otherUser.Id);

        await _meetingUtil.AddMeetingRecord(meetingRecord1);
        await _meetingUtil.AddMeetingRecord(meetingRecord2);
        await _meetingUtil.AddMeetingRecord(meetingRecord3);

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(2);
            var creatorList = response.Data.Records.Select(x => x.MeetingCreator).ToList();
            creatorList.ShouldContain(otherUser.UserName);
            creatorList.ShouldContain(testCurrentUser.UserName);
        });
    }

    [Fact]
    public async Task CanGetNewMeetingRecord()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

        var testRecord1 = await _meetingUtil.GenerateMeetingRecordAsync(meetingDto, "mock url1");
        await _meetingUtil.AddMeetingRecordAsync(testRecord1);
        var testRecord2 = await _meetingUtil.GenerateMeetingRecordAsync(meetingDto, "mock url12");
        await _meetingUtil.AddMeetingRecordAsync(testRecord2);
        var testRecord3 = await _meetingUtil.GenerateMeetingRecordAsync(meetingDto, "mock url3");
        await _meetingUtil.AddMeetingRecordAsync(testRecord3);

        var meetingRecords = await _meetingUtil.GetMeetingRecordsByMeetingIdAsync(meetingDto.Id);
        var test3 = meetingRecords.FirstOrDefault(x => x.Url == "mock url3");
        var newMeetingRecord = await _meetingUtil.GetMeetingRecordByMeetingIdAsync(meetingDto.Id);
        newMeetingRecord.CreatedDate.ShouldBe(test3.CreatedDate);
    }

    [Theory]
    [InlineData("mock url1")]
    [InlineData("mock url2")]
    public async Task CanMeetingRecordShouldBeValue( string url)
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
        var meetingRecord = await _meetingUtil.GenerateMeetingRecordAsync(meetingDto,url);
        await _meetingUtil.AddMeetingRecordAsync(meetingRecord);
        var response = await _meetingUtil.GetMeetingRecordByMeetingIdAsync(meetingDto.Id);
        response.ShouldNotBeNull();
        response.Url.ShouldBe(url);
    }

    [Fact]
    public async Task CanMeetingRecordResponseShouldBeValue()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
        var meetingRecord = await _meetingUtil.GenerateMeetingRecordAsync(meetingDto);
        await _meetingUtil.AddMeetingRecordAsync(meetingRecord);
        var dbMeetingRecord = await _meetingUtil.GetMeetingRecordByMeetingIdAsync(meetingDto.Id);
        dbMeetingRecord.RecordType.ShouldBe(MeetingRecordType.OnRecord);

        var response = await _meetingUtil.StorageMeetingRecordVideoByMeetingIdAsync(meetingDto.Id);
        response.Code.ShouldBe(HttpStatusCode.OK);

        var newMeetingRecord = await _meetingUtil.GetMeetingRecordByMeetingIdAsync(meetingDto.Id);
        newMeetingRecord.Url.ShouldBe("mock url");
        newMeetingRecord.RecordType.ShouldBe(MeetingRecordType.EndRecord);
        newMeetingRecord.MeetingId.ShouldBe(meetingDto.Id);
    }

    [Fact]
    public async Task WhenMeetingRecordIsNullShouldBeThrow()
    {
        await _meetingUtil.StorageMeetingRecordVideoByMeetingIdAsync(Guid.NewGuid())
            .ShouldThrowAsync<MeetingRecordNotFoundException>();
    }
}