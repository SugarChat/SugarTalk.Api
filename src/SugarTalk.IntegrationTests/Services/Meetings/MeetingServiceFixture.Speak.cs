using Xunit;
using System;
using Autofac;
using Shouldly;
using System.Linq;
using NSubstitute;
using Mediator.Net;
using System.Threading;
using SugarTalk.Core.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Validators.Commands;
using SugarTalk.Core.Services.Http.Clients;
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
        const string roomNumber = "123456";
        const string trackId = "1707894406";
        var starTime = DateTimeOffset.Parse("2024-01-01 00:00:00 +00:00").ToUnixTimeSeconds();
        var endTime = DateTimeOffset.Parse("2024-01-01 00:01:00 +00:00").ToUnixTimeSeconds();
        
        await RunWithUnitOfWork<ICurrentUser, IRepository>(async (currentUser, repository) =>
        {
            if (isUpdate)
                await repository.InsertAsync(new MeetingSpeakDetail
                {
                    TrackId = trackId,
                    FilePath = "test",
                    EgressId = "test",
                    Id = speakDetailId,
                    RoomNumber = roomNumber,
                    SpeakStartTime = starTime,
                    UserId = currentUser.Id.Value,
                    MeetingRecordId = Guid.NewGuid()
                });
        });

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var response = await mediator.SendAsync<RecordMeetingSpeakCommand, RecordMeetingSpeakResponse>(new RecordMeetingSpeakCommand
            {
                Id = isUpdate ? speakDetailId : null,
                MeetingRecordId = Guid.NewGuid(),
                TrackId = trackId,
                RoomNumber = roomNumber,
                SpeakStartTime = isUpdate ? null : starTime,
                SpeakEndTime = isUpdate ? endTime : null
            });

            response.Data.ShouldNotBeNull();
            response.Data.UserId.ShouldBe(1);
            response.Data.TrackId.ShouldBe(trackId);
            response.Data.EgressId.ShouldNotBeNull();
            response.Data.RoomNumber.ShouldBe(roomNumber);

            var speakDetails = await repository.Query<MeetingSpeakDetail>().ToListAsync().ConfigureAwait(false);
            
            speakDetails.Count.ShouldBe(1);
            speakDetails.First().UserId.ShouldBe(1);
            speakDetails.First().TrackId.ShouldBe(trackId);
            speakDetails.First().EgressId.ShouldNotBeNull();
            speakDetails.First().RoomNumber.ShouldBe(roomNumber);
            
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
        }, builder =>
        {
            var liveKitClient = Substitute.For<ILiveKitClient>();
            
            liveKitClient.StartTrackCompositeEgressAsync(Arg.Any<StartTrackCompositeEgressRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new StartEgressResponseDto
                {
                    EgressId = Guid.NewGuid().ToString()
                });

            liveKitClient.StopEgressAsync(Arg.Any<StopEgressRequestDto>(), Arg.Any<CancellationToken>())
                .Returns("Success");
            
            builder.RegisterInstance(liveKitClient);
        });
    }
    
    [Theory]
    [InlineData(null, null, null, null, null, null,false)]
    [InlineData(null, "123456", 1707894406, null, null, null, false)]
    [InlineData(null, "123456", null, 1707894406, null, null, false)]
    [InlineData(null, "123456", 1707894406, 1707894406, null, null, false)]
    [InlineData(null, "123456", null, null, null, null, false)]
    [InlineData("4d992f24-346b-48ce-a855-1b10cfd4bd6e", "123456", null, 1707894406, null, null, false)]
    [InlineData("4d992f24-346b-48ce-a855-1b10cfd4bd6e", "123456", 1707894406, null, null, null, false)]
    [InlineData(null, "123456", 1707894406, null, "test", "7a7f6ff4-1832-4f5f-b059-c2ebdc6196a3", true)]
    [InlineData("4d992f24-346b-48ce-a855-1b10cfd4bd6e", "123456", null, 1707894406, "test", "7a7f6ff4-1832-4f5f-b059-c2ebdc6196a3", true)]
    public async Task CanValidateRecordMeetingSpeak(string id, string roomNumber, int? startTime, int? endTime, string trackId, string recordId, bool isValid)
    {
        var command = new RecordMeetingSpeakCommand
        {
            Id = string.IsNullOrEmpty(id) ? null : Guid.Parse(id),
            RoomNumber = roomNumber,
            SpeakStartTime = startTime,
            SpeakEndTime = endTime,
            TrackId = trackId,
            MeetingRecordId = string.IsNullOrEmpty(recordId) ? Guid.Empty : Guid.Parse(recordId)
        };
        
        var validator = new RecordMeetingSpeakCommandValidator();

        var result = await validator.ValidateAsync(command);
        
        result.IsValid.ShouldBe(isValid);
    }
}