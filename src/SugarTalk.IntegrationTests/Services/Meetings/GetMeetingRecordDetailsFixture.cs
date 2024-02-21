using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.IntegrationTests.TestBaseClasses;
using SugarTalk.Messages.Dto.Translation;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Enums.Meeting.Speak;
using SugarTalk.Messages.Enums.Meeting.Summary;
using SugarTalk.Messages.Requests.Meetings;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public class GetMeetingRecordDetailsFixture : MeetingFixtureBase
{
    // [Theory]
    // [InlineData("e581c3fb-8ed0-4b8f-82fc-2453b84d403e", "08db98a9-6a57-48dc-86e5-31bc1c3a8e92", "AI Meeting", "1234567890", "https:/ai meetings/1234567890", "AI Meeting 會議機要", "AI Meeting 會議sugarTalk", "AI Meeting 會議history")]
    // [InlineData("e19489c5-25d8-42e0-a30a-b5bb814cd7de", "08db98b4-3a60-40ac-8d81-c162c97916be", "Solar Meeting", "0123456789", "https:/solar meetings/0123456789", "Solar Meeting 會議機要", "Solar Meeting 會議sugarTalk", "Solar Meeting 會議history")]
    // [InlineData("7de02c6d-198d-4c8f-81c0-b3ebd5041b9a", "08db994a-adac-4591-8181-3cc86099f525", "BI Meeting", "9012345678", "https:/bi meetings/9012345678", "BI Meeting 會議機要", "BI Meeting 會議sugarTalk", "BI Meeting 會議history")]
    // public async Task CanGetMeetingRecordDetailsData(string recordId, string meetingId, string meetingTitile,
    //     string meetingNumber, string meetingUrl, string meetingSummary, string meetingContent1, string meetingContent2)
    // {
    //     var request = new GetMeetingRecordDetailsRequest()
    //     {
    //         Id = Guid.Parse(recordId)
    //     };
    //     var meetingInfo = new Meeting
    //     {
    //         Id = Guid.Parse(meetingId),
    //         Title = meetingTitile,
    //         MeetingNumber = meetingNumber,
    //         StartDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    //         EndDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    //         AppointmentType = MeetingAppointmentType.Appointment
    //     };
    //     var meetingRecord = new MeetingRecord
    //     {
    //         Id = Guid.Parse(recordId),
    //         MeetingId = Guid.Parse(meetingId),
    //         Url = meetingUrl,
    //         CreatedDate = DateTimeOffset.Now,
    //     };
    //     var meetingRecordSummary = new MeetingSummary()
    //     {
    //         Id = 1,
    //         RecordId = Guid.Parse(recordId),
    //         MeetingNumber = meetingNumber,
    //         SpeakIds = "1",
    //         OriginText = "测试",
    //         Status = SummaryStatus.Pending,
    //         TargetLanguage = TranslationLanguage.ZhCn,
    //         CreatedDate = DateTimeOffset.Now,
    //         Summary = meetingSummary
    //     };
    //     var meetingRecordDetails = new List<MeetingSpeakDetail>
    //     {
    //         new MeetingSpeakDetail
    //         {
    //             Id = 1,
    //             MeetingRecordId = Guid.Parse(recordId),
    //             UserId = 1,
    //             SpeakStartTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(),
    //             SpeakContent = meetingContent1,
    //             SpeakStatus = SpeakStatus.Speaking,
    //             CreatedDate = DateTimeOffset.Now,
    //             TrackId = "1",
    //             EgressId = "1",
    //             MeetingNumber = meetingNumber,
    //             FilePath = "http://localhost:5000/api/v1/meeting/record/download/1"
    //         },
    //         new MeetingSpeakDetail
    //         {
    //             Id = 2,
    //             MeetingRecordId = Guid.Parse(recordId),
    //             UserId = 2,
    //             SpeakStartTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(),
    //             SpeakContent = meetingContent2,
    //             SpeakStatus = SpeakStatus.Speaking,
    //             CreatedDate = DateTimeOffset.Now,
    //             TrackId = "2",
    //             EgressId = "2",
    //             MeetingNumber = meetingNumber,
    //             FilePath = "http://localhost:5000/api/v1/meeting/record/download/2"
    //         }
    //     };
    //
    //     await RunWithUnitOfWork<IRepository>(async repository =>
    //     {
    //         await repository.InsertAsync(meetingInfo);
    //         await repository.InsertAsync(meetingRecord);
    //         await repository.InsertAsync(meetingRecordSummary);
    //         await repository.InsertAllAsync(meetingRecordDetails);
    //     });
    //
    //     await Run<IMediator>(async mediator =>
    //     {
    //         var result = await mediator.RequestAsync<GetMeetingRecordDetailsRequest, GetMeetingRecordDetailsResponse>(request).ConfigureAwait(false);
    //
    //         result.Data.ShouldNotBeNull();
    //         result.Data.MeetingTitle.ShouldBe(meetingTitile);
    //         result.Data.MeetingNumber.ShouldBe(meetingNumber);
    //         result.Data.MeetingStartDate.ShouldBe(meetingInfo.StartDate);
    //         result.Data.MeetingEndDate.ShouldBe(meetingInfo.EndDate);
    //         result.Data.Url.ShouldBe(meetingUrl);
    //         result.Data.Summary.ShouldBe(meetingSummary);
    //         
    //         result.Data.MeetingRecordDetail.ShouldNotBeNull();
    //         
    //         result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 1).ShouldNotBeNull();
    //         result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 1)?.SpeakContent.ShouldBe(meetingContent1);
    //         result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 1)?.SpeakStartTime
    //             .ShouldBe(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
    //         
    //         result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 2).ShouldNotBeNull();
    //         result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 2)?.SpeakContent.ShouldBe(meetingContent2);
    //         result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 2)?.SpeakStartTime
    //             .ShouldBe(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
    //     });
    // }
}