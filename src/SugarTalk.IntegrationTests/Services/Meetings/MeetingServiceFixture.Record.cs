using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Enums.Meeting;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Fact]
    public async Task CanGetMeetingRecord()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
        var meetingRecord=  await _meetingUtil.ScheduleMeetingRecordAsync(meetingDto);
        await _meetingUtil.AddMeetingRecordAsync(meetingRecord);
        var response= await _meetingUtil.GetMeetingRecordByMeetingIdAsync(meetingDto.Id);
        response.ShouldNotBeNull();
    }

    [Fact]
    public async Task CanGetNewMeetingRecord()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
        
        var testRecord1=  await _meetingUtil.ScheduleMeetingRecordAsync(meetingDto,"TestId1","TestUrl1");
        await _meetingUtil.AddMeetingRecordAsync(testRecord1);
        var testRecord2=  await _meetingUtil.ScheduleMeetingRecordAsync(meetingDto,"TestId2","TestUrl2");
        await _meetingUtil.AddMeetingRecordAsync(testRecord2);
        var testRecord3=  await _meetingUtil.ScheduleMeetingRecordAsync(meetingDto,"TestId3","TestUrl3");
        await _meetingUtil.AddMeetingRecordAsync(testRecord3);

        var meetingRecords = await _meetingUtil.GetMeetingRecordsByMeetingIdAsync(meetingDto.Id);
        var test3 = meetingRecords.FirstOrDefault(x => x.EgressId == "TestId3");
        var newMeetingRecord = await _meetingUtil.GetMeetingRecordByMeetingIdAsync(meetingDto.Id);
        newMeetingRecord.CreatedDate.ShouldBe(test3.CreatedDate);
    }
    
    [Theory]
    [InlineData("测试Id1","测试Url1")]
    [InlineData("测试Id2","测试Url2")]
    public async Task CanMeetingRecordShouldBeValue(string egressId,string url)
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
        var meetingRecord=  await _meetingUtil.ScheduleMeetingRecordAsync(meetingDto,egressId,url);
        await _meetingUtil.AddMeetingRecordAsync(meetingRecord);
        var response= await _meetingUtil.GetMeetingRecordByMeetingIdAsync(meetingDto.Id);
        response.ShouldNotBeNull();
        response.EgressId.ShouldBe(egressId);
        response.Url.ShouldBe(url);
    }
    
    [Fact]
    public async Task CanMeetingRecordResponseShouldBeValue()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
        var meetingRecord=  await _meetingUtil.ScheduleMeetingRecordAsync(meetingDto);
        await _meetingUtil.AddMeetingRecordAsync(meetingRecord);
        var dbMeetingRecord= await _meetingUtil.GetMeetingRecordByMeetingIdAsync(meetingDto.Id);
        dbMeetingRecord.RecordType.ShouldBe(MeetingRecordType.OnRecord);

        var response=await  _meetingUtil.StorageMeetingRecordVideoByMeetingIdAsync(meetingDto.Id);
        response.Code.ShouldBe(HttpStatusCode.OK);
        
        var newMeetingRecord = await _meetingUtil.GetMeetingRecordByMeetingIdAsync(meetingDto.Id);
        newMeetingRecord.Url.ShouldBe("测试更新Url");
        newMeetingRecord.RecordType.ShouldBe(MeetingRecordType.EndRecord);
        newMeetingRecord.MeetingId.ShouldBe(meetingDto.Id);
    }

    [Fact]
    public async Task WhenMeetingRecordTypeIsEndRecordShouldBeThrow()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
        var testMeetingRecord = new MeetingRecord
        {
            CreatedDate = DateTimeOffset.Now,
            EgressId = "TestId",
            Url = "TestUrl",
            MeetingId = meetingDto.Id,
            RecordType = MeetingRecordType.EndRecord
        };
        await _meetingUtil.AddMeetingRecordAsync(testMeetingRecord);
        await  _meetingUtil.StorageMeetingRecordVideoByMeetingIdAsync(meetingDto.Id).ShouldThrowAsync<MeetingRecordNotOpenException>();
    }

    [Fact]
    public async Task WhenMeetingRecordIsNullShouldBeThrow()
    {
        await  _meetingUtil.StorageMeetingRecordVideoByMeetingIdAsync(Guid.NewGuid()).ShouldThrowAsync<MeetingRecordNotFoundException>();
    }
}