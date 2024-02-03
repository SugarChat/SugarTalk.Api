using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Mediator.Net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Requests.Meetings.User;
using SugarTalk.Messages.Requests.Speech;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Fact]
    public async Task ShouldSaveMeetingSpeech()
    {
        var currentUser = new TestCurrentUser();
        
        var meetingId = Guid.NewGuid();
        var command = new SaveMeetingAudioCommand
        {
            MeetingId = meetingId,
            AudioForBase64 = "巴卡巴卡巴卡"
        };

        await _meetingUtil.AddMeetingUserSetting(Guid.NewGuid(), currentUser.Id.Value, meetingId: meetingId, 
            SpeechTargetLanguageType.Cantonese, CantoneseToneType.WanLungNeural);
        
        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            await mediator.SendAsync(command);

            var meetingSpeech = await repository.QueryNoTracking<MeetingSpeech>().SingleAsync(CancellationToken.None);
            
            meetingSpeech.MeetingId.ShouldBe(command.MeetingId);
            meetingSpeech.OriginalText.ShouldBe("text");
            meetingSpeech.UserId.ShouldBe(currentUser.Id.Value);
        }, builder =>
        {
            var speechClient = Substitute.For<ISpeechClient>();

            speechClient.TranslateTextAsync(Arg.Any<TextTranslationDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "translated_text" });
            
            speechClient.GetTextFromAudioAsync(Arg.Any<SpeechToTextDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "text" });
            
            speechClient.GetAudioFromTextAsync(Arg.Any<TextToSpeechDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "text.url" });

            builder.RegisterInstance(speechClient);
        });
    }

    [Fact]
    public async Task ShouldGetMeetingSpeechList()
    {
        var meetingId = Guid.NewGuid();

        var now = DateTimeOffset.Now;
        
        var user1 = await _accountUtil.AddUserAccount("greg", "123456").ConfigureAwait(false);
        var user2 = await _accountUtil.AddUserAccount("mars", "123456").ConfigureAwait(false);

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            await repository.InsertAllAsync(new List<MeetingSpeech>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    MeetingId = meetingId,
                    OriginalText = "text",
                    CreatedDate = now,
                    UserId = user1.Id
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    MeetingId = meetingId,
                    OriginalText = "text2",
                    CreatedDate = now.AddHours(1),
                    UserId = user1.Id
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    MeetingId = meetingId,
                    OriginalText = "text3",
                    UserId = user2.Id,
                    Status = SpeechStatus.Cancelled
                }
            }, CancellationToken.None);

            await _meetingUtil.AddMeetingUserSetting(Guid.NewGuid(), user1.Id, meetingId,
                SpeechTargetLanguageType.Cantonese, CantoneseToneType.WanLungNeural);
        });

        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            var response = await mediator.RequestAsync<GetMeetingAudioListRequest, GetMeetingAudioListResponse>(
                new GetMeetingAudioListRequest
                {
                    MeetingId = meetingId, 
                    LanguageType = SpeechTargetLanguageType.Cantonese,
                    FilterHasCanceledAudio = true
                });

            response.Data.Count.ShouldBe(2);
            
            response.Data.First().UserId.ShouldBe(user1.Id);
            response.Data.First().UserName.ShouldBe(user1.UserName);
            response.Data.First().MeetingId.ShouldBe(meetingId);
            response.Data.First().OriginalText.ShouldBe("text");
            response.Data.First().VoiceUrl.ShouldBe("test.url");
            response.Data.First().TranslatedText.ShouldBe("translated_text");
        }, builder =>
        {
            var speechClient = Substitute.For<ISpeechClient>();

            speechClient.TranslateTextAsync(Arg.Any<TextTranslationDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "translated_text" });
            
            speechClient.GetTextFromAudioAsync(Arg.Any<SpeechToTextDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "text" });
            
            speechClient.GetAudioFromTextAsync(Arg.Any<TextToSpeechDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "test.url" });

            builder.RegisterInstance(speechClient);
        });
    }

    [Fact]
    public async Task ShouldUpdateMeetingSpeech()
    {
        var currentUser = new TestCurrentUser();

        var speechId = Guid.NewGuid();
        
        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            await repository.InsertAllAsync(new List<MeetingSpeech>
            {
                new()
                {
                    Id = speechId,
                    MeetingId = Guid.NewGuid(),
                    OriginalText = "text",
                    UserId = currentUser.Id.Value,
                    Status = SpeechStatus.UnViewed
                }
            }, CancellationToken.None);
        });
        
        await Run<IMediator, IRepository>(async (mediator, db) =>
        {
            await mediator.SendAsync<UpdateMeetingSpeechCommand, UpdateMeetingSpeechResponse>(new UpdateMeetingSpeechCommand
            {
                MeetingSpeechId = speechId,
                Status = SpeechStatus.Viewed
            });

            var meetingSpeech = await db.Query<MeetingSpeech>().ToListAsync(CancellationToken.None);

            meetingSpeech.First().Status.ShouldBe(SpeechStatus.Viewed);
        });
    }
    
    [Fact]
    public async Task CanGetMeetingUserSettingWhenJoinMeeting()
    {
        var currentUser = new TestCurrentUser();
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var meeting = await _meetingUtil.GetMeeting(scheduleMeetingResponse.Data.MeetingNumber);

        await _meetingUtil.JoinMeeting(meeting.MeetingNumber);
        
        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            var response = await mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(new JoinMeetingCommand
            {
                MeetingNumber = scheduleMeetingResponse.Data.MeetingNumber,
                IsMuted = false
            });

            response.Data.MeetingUserSetting.UserId.ShouldBe(currentUser.Id.Value);
            
            var responseUserSetting = await mediator.RequestAsync<GetMeetingUserSettingRequest, GetMeetingUserSettingResponse>(new GetMeetingUserSettingRequest
            {
                MeetingId = meeting.Id
            });
            
            responseUserSetting.Data.UserId.ShouldBe(currentUser.Id.Value);
        }, builder =>
        {
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(liveKitServerUtilService);
        });
    }
    
    [Fact]
    public async Task CanGetAppointmentMeetings()
    {
        var currentUser = new TestCurrentUser();
        
        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            await repository.InsertAllAsync(new List<Meeting>
                {
                    new ()
                    {
                        Title = "会议1",
                        TimeZone = "",
                        SecurityCode = "",
                        StartDate = DateTimeOffset.Parse("2024-02-02T03:10:00.825Z").ToUnixTimeMilliseconds(),
                        EndDate = DateTimeOffset.Parse("2024-02-02T03:11:00.825Z").ToUnixTimeMilliseconds(),
                        AppointmentType = MeetingAppointmentType.Appointment,
                        IsMuted = true,
                        IsRecorded = true,
                        MeetingNumber = "",
                        MeetingStreamMode = 0 
                    },
                    new ()
                    {
                        Title = "会议2",
                        TimeZone = "",
                        SecurityCode = "",
                        StartDate = DateTimeOffset.Parse("2024-02-02T03:11:00.825Z").ToUnixTimeMilliseconds(),
                        EndDate = DateTimeOffset.Parse("2024-02-02T03:12:00.825Z").ToUnixTimeMilliseconds(),
                        AppointmentType = MeetingAppointmentType.Appointment,
                        IsMuted = true,
                        IsRecorded = true,
                        MeetingNumber = "",
                        MeetingStreamMode = 0 
                    },
                    new ()
                    {
                        Title = "会议3",
                        TimeZone = "",
                        SecurityCode = "",
                        StartDate = DateTimeOffset.Parse("2024-02-02T03:12:00.825Z").ToUnixTimeMilliseconds(),
                        EndDate = DateTimeOffset.Parse("2024-02-02T03:13:00.825Z").ToUnixTimeMilliseconds(),
                        AppointmentType = MeetingAppointmentType.Appointment,
                        IsMuted = true,
                        IsRecorded = true,
                        MeetingNumber = "",
                        MeetingStreamMode = 0 
                    },
                }
           );
            
            // var response = await mediator.RequestAsync<GetAppointmentMeetingsRequest, GetAppointmentMeetingsResponse>(
            //     new GetAppointmentMeetingsRequest { Page = 1, PageSize = 3 });
            //
            // response.Data.Count.ShouldBe(3);
            var modelWhiteList = await repository.Query<Meeting>().FirstOrDefaultAsync();
            
        });

    }
}