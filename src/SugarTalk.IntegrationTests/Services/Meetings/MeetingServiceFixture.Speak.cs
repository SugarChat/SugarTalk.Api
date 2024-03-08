using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Shouldly;
using System.Linq;
using System.Text;
using NSubstitute;
using Mediator.Net;
using System.Threading;
using SugarTalk.Core.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Validators.Commands;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Enums.Meeting.Speak;
using SugarTalk.Messages.Commands.Meetings.Speak;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ShouldRecordMeetingSpeak(bool isUpdate)
    {
        const int speakDetailId = 1;
        const string roomNumber = "123456";
        const string trackId = "1707894406"; 
        const string fileUrl = "http://smartiestest.oss-cn-hongkong.aliyuncs.com/20231225/a2421020-f81a-4ade-9628-1b5b1414fd1d.wav?Expires=253402300799&OSSAccessKeyId=LTAI5tCcEbGZDf6m55ccFE7V&Signature=wAXV6gVtwpNP62mzHAU%2BD%2BQLH8c%3D";
        const string audioContent = "0123123测试测试";
        var fileContent = Encoding.UTF8.GetBytes(audioContent);
        var starTime = DateTimeOffset.Parse("2024-01-01 00:00:00 +00:00").ToUnixTimeSeconds();
        var endTime = DateTimeOffset.Parse("2024-01-01 00:01:00 +00:00").ToUnixTimeSeconds();
        
        await RunWithUnitOfWork<ICurrentUser, IRepository>(async (currentUser, repository) =>
        {
            if (isUpdate)
                await repository.InsertAsync(new MeetingSpeakDetail
                {
                    TrackId = trackId,
                    Id = speakDetailId,
                    MeetingNumber = roomNumber,
                    SpeakStartTime = starTime,
                    UserId = currentUser.Id.Value,
                    MeetingRecordId = Guid.NewGuid()
                });
        });

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var response = await mediator.SendAsync<RecordMeetingSpeakCommand, RecordMeetingSpeakResponse>(
                new RecordMeetingSpeakCommand
                {
                    Id = isUpdate ? speakDetailId : null,
                    MeetingRecordId = Guid.NewGuid(),
                    TrackId = trackId,
                    MeetingNumber = roomNumber,
                    SpeakStartTime = isUpdate ? null : starTime,
                    SpeakEndTime = isUpdate ? endTime : null
                });

            response.Data.ShouldNotBeNull();
            response.Data.UserId.ShouldBe(1);
            response.Data.TrackId.ShouldBe(trackId);
            response.Data.MeetingNumber.ShouldBe(roomNumber);

            var speakDetails = await repository.Query<MeetingSpeakDetail>().ToListAsync().ConfigureAwait(false);
            
            speakDetails.Count.ShouldBe(1);
            speakDetails.First().UserId.ShouldBe(1);
            speakDetails.First().TrackId.ShouldBe(trackId);
            speakDetails.First().MeetingNumber.ShouldBe(roomNumber);
            
            if (isUpdate)
            {
                response.Data.Id.ShouldBe(speakDetailId);
                response.Data.SpeakEndTime.ShouldBe(endTime);
                speakDetails.First().Id.ShouldBe(speakDetailId);
                speakDetails.First().SpeakEndTime.ShouldBe(endTime);
                speakDetails.First().SpeakStatus.ShouldBe(SpeakStatus.End);

                switch (speakDetails.First().FileTranscriptionStatus)
                {
                    case FileTranscriptionStatus.Completed:
                        speakDetails.First().OriginalContent.ShouldBe(audioContent);
                        break;
                    case FileTranscriptionStatus.Pending:
                        speakDetails.First().OriginalContent.ShouldBeNull();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();
            var openAiService = Substitute.For<IOpenAiService>();

            liveKitClient.StartTrackCompositeEgressAsync(Arg.Any<StartTrackCompositeEgressRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new StartEgressResponseDto
                {
                    EgressId = Guid.NewGuid().ToString()
                });

            liveKitClient.StopEgressAsync(Arg.Any<StopEgressRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new StopEgressResponseDto());

            liveKitClient.GetEgressInfoListAsync
                    (Arg.Any<GetEgressRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new GetEgressInfoListResponseDto
                {
                    EgressItems = new List<EgressItemDto>
                    {
                        new EgressItemDto()
                        {
                            EgressId = "test",
                            File = new FileDetails()
                            {
                                Filename = "a2421020-f81a-4ade-9628-1b5b1414fd1d.wav",
                                StartedAt = "2024-01-01T00:00:00Z",
                                EndedAt = "2024-01-01T00:00:12Z",
                                Duration = "12s",
                                Size = "1234567890",
                                Location = fileUrl
                            }
                        }
                    }
                });

            openAiService.TranscriptionAsync(Arg.Any<byte[]>(), Arg.Any<TranscriptionLanguage?>(),
                Arg.Any<long>(), Arg.Any<long>(), Arg.Any<TranscriptionFileType>(), Arg.Any<TranscriptionResponseFormat>(),
                Arg.Any<CancellationToken>()).Returns(audioContent);

            openAiService.GetAsync<byte[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(fileContent);
            
            liveKitServerUtilService.GenerateTokenForRecordMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(liveKitClient);
            builder.RegisterInstance(openAiService);
            builder.RegisterInstance(liveKitServerUtilService);
        });
    }
    
    [Theory]
    [InlineData(null, null, null, null, null, null, false)]
    [InlineData(null, "123456", 1707894406, null, null, null, false)]
    [InlineData(null, "123456", null, 1707894406, null, null, false)]
    [InlineData(null, "123456", 1707894406, 1707894406, null, null, false)]
    [InlineData(null, "123456", null, null, null, null, false)]
    [InlineData(2, "123456", null, 1707894406, null, null, false)]
    [InlineData(3, "123456", 1707894406, null, null, null, false)]
    [InlineData(null, "123456", 1707894406, null, "test", "7a7f6ff4-1832-4f5f-b059-c2ebdc6196a3", true)]
    [InlineData(4, "123456", null, 1707894406, "test", "7a7f6ff4-1832-4f5f-b059-c2ebdc6196a3", true)]
    public async Task CanValidateRecordMeetingSpeak(int? id, string roomNumber, int? startTime, int? endTime, string trackId, string recordId, bool isValid)
    {
        var command = new RecordMeetingSpeakCommand
        {
            Id = id,
            MeetingNumber = roomNumber,
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