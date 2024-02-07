using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Fact]
    public async Task ShouldGetMeetingRecord()
    {
        var otherUser = await _accountUtil.AddUserAccount("user1", "123456").ConfigureAwait(false);

        var meeting1 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "1"
        };
        var meeting2 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "2"
        };
        var meeting3 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "3"
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
            var response = await mediator
                .RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(
                    new GetCurrentUserMeetingRecordRequest()).ConfigureAwait(false);
            response.MeetingRecordList.Count.ShouldBe(2);
            response.MeetingRecordList[0].MeetingId.ShouldBe(meeting1.Id);
        });
    }
}