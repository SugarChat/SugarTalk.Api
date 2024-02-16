using System;
using System.Net;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Fact]
    public async Task CanGetMeetingRecord()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
        var response = await _meetingUtil.AddMeetingRecordAsync(meetingDto.Id, "测试Url");
        response.Code.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CanMeetingRecordCountShouldBeValue()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
        await _meetingUtil.AddMeetingRecordAsync(meetingDto.Id, "测试Url");
        await _meetingUtil.AddMeetingRecordAsync(meetingDto.Id, "测试Url");
        await _meetingUtil.AddMeetingRecordAsync(meetingDto.Id, "测试Url");
        // var count = await _meetingUtil.GetMeetingRecordsCountAsync(meetingDto.Id);
        //ToDo
       // count.ShouldBe(3);
    }

    [Fact]
    public async Task CanMeetingRecordRecordNumberShouldBeValue()
    {
    }
}