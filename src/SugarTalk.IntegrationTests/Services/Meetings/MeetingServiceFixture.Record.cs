using System;
using System.Linq;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto;
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
            Url = "mock url1"
        };

        var meetingRecord2 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1.Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-1),
            Url = "mock url2"
        };

        var meetingRecord3 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting2.Id,
            Url = "mock url3"
        };

        var meetingRecord4 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting3.Id,
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
            Url = "mock url3"
        };

        var meetingRecord2 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting3.Id,
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
}