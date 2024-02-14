using Xunit;
using System;
using Shouldly;
using System.Linq;
using Mediator.Net;
using SugarTalk.Core.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Validators.Commands;
using SugarTalk.Messages.Enums.Meeting.Speak;
using SugarTalk.Messages.Commands.Meetings.Speak;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ShouldRecordMeetingSpeak(bool isUpdate)
    {
        var speakDetailId = Guid.NewGuid();
        var starTime = DateTimeOffset.Parse("2024-01-01 00:00:00 +00:00").ToUnixTimeSeconds();
        var endTime = DateTimeOffset.Parse("2024-01-01 00:01:00 +00:00").ToUnixTimeSeconds();
        
        await RunWithUnitOfWork<ICurrentUser, IRepository>(async (currentUser, repository) =>
        {
            if (isUpdate)
                await repository.InsertAsync(new MeetingSpeakDetail
                {
                    Id = speakDetailId,
                    SpeakStartTime = starTime,
                    MeetingId = Guid.NewGuid(),
                    MeetingSubId = Guid.NewGuid(),
                    UserId = currentUser.Id.Value,
                    MeetingRecordId = Guid.NewGuid()
                });
        });

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var response = await mediator.SendAsync<RecordMeetingSpeakCommand, RecordMeetingSpeakResponse>(new RecordMeetingSpeakCommand
            {
                Id = isUpdate ? speakDetailId : null,
                MeetingId = Guid.NewGuid(),
                MeetingSubId = Guid.NewGuid(),
                SpeakStartTime = isUpdate ? null : starTime,
                SpeakEndTime = isUpdate ? endTime : null
            });

            response.Data.ShouldNotBeNull();
            response.Data.UserId.ShouldBe(1);

            var speakDetails = await repository.Query<MeetingSpeakDetail>().ToListAsync().ConfigureAwait(false);
            
            speakDetails.Count.ShouldBe(1);
            speakDetails.First().UserId.ShouldBe(1);
            
            if (isUpdate)
            {
                response.Data.Id.ShouldBe(speakDetailId);
                response.Data.SpeakEndTime.ShouldBe(endTime);
                speakDetails.First().Id.ShouldBe(speakDetailId);
                speakDetails.First().SpeakEndTime.ShouldBe(endTime);
                speakDetails.First().SpeakStatus.ShouldBe(SpeakStatus.End);
            }
            else
            {
                response.Data.SpeakStartTime.ShouldBe(starTime);
                speakDetails.First().SpeakStartTime.ShouldBe(starTime);
                speakDetails.First().SpeakStatus.ShouldBe(SpeakStatus.Speaking);
            }
        });
    }
    
    [Theory]
    [InlineData(null, null, null, null, false)]
    [InlineData(null, "48771c4e-ce54-4a6e-a150-c9ba7c6229fd", 1707894406, null, true)]
    [InlineData(null, "48771c4e-ce54-4a6e-a150-c9ba7c6229fd", null, 1707894406, false)]
    [InlineData(null, "48771c4e-ce54-4a6e-a150-c9ba7c6229fd", 1707894406, 1707894406, true)]
    [InlineData(null, "48771c4e-ce54-4a6e-a150-c9ba7c6229fd", null, null, false)]
    [InlineData("4d992f24-346b-48ce-a855-1b10cfd4bd6e", "48771c4e-ce54-4a6e-a150-c9ba7c6229fd", null, 1707894406, true)]
    public async Task CanValidateRecordMeetingSpeak(string id, string meetingId, int? startTime, int? endTime, bool isValid)
    {
        var command = new RecordMeetingSpeakCommand
        {
            Id = string.IsNullOrEmpty(id) ? null : Guid.Parse(id),
            MeetingId = string.IsNullOrEmpty(meetingId) ? Guid.Empty : Guid.Parse(meetingId),
            SpeakStartTime = startTime,
            SpeakEndTime = endTime
        };
        
        var validator = new RecordMeetingSpeakCommandValidator();

        var result = await validator.ValidateAsync(command);
        
        result.IsValid.ShouldBe(isValid);
    }
}