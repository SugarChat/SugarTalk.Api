using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Google.Cloud.Translation.V2;
using Mediator.Net;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Dto.Smarties;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Requests.Meetings.User;
using SugarTalk.Messages.Requests.Speech;
using Xunit;
using TranslationClient = Google.Cloud.Translation.V2.TranslationClient;

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
        
        await _meetingUtil.AddMeetingChatRoomSetting(1, meetingId, currentUser.Id.Value, 
            "1111", SpeechTargetLanguageType.Cantonese, SpeechTargetLanguageType.Mandarin);
        
        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            await mediator.SendAsync(command);

            var meetingSpeech = await repository.QueryNoTracking<MeetingSpeech>().SingleAsync(CancellationToken.None);
            
            meetingSpeech.MeetingId.ShouldBe(command.MeetingId);
            meetingSpeech.OriginalText.ShouldBe("text");
            meetingSpeech.UserId.ShouldBe(currentUser.Id.Value);
            
            var meetingChatVoiceRecord = await repository.QueryNoTracking<MeetingChatVoiceRecord>().SingleAsync(CancellationToken.None);
            
            meetingChatVoiceRecord.VoiceUrl.ShouldBe("我是ea产生的voiceUrl");
            meetingChatVoiceRecord.TranslatedText.ShouldBe("translated_text");
            meetingChatVoiceRecord.GenerationStatus.ShouldBe(ChatRecordGenerationStatus.Completed);
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            var speechClient = Substitute.For<ISpeechClient>();

            speechClient.TranslateTextAsync(Arg.Any<TextTranslationDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "translated_text" });
            
            speechClient.GetTextFromAudioAsync(Arg.Any<SpeechToTextDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "text" });
            
            speechClient.GetAudioFromTextAsync(Arg.Any<TextToSpeechDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "text.url" });
            
            speechClient.SpeechInferenceAsync(Arg.Any<SpeechInferenceDto>(), CancellationToken.None)
                .Returns(new SpeechInferenceResponseDto
                {
                    Result = new SpeechInferenceResultDto { Url = "我是ea产生的voiceUrl"}
                });

            builder.RegisterInstance(speechClient);
            builder.RegisterInstance(openAiService);
        });
    }

    [Fact]
    public async Task ShouldGetMeetingSpeechList()
    {
        var now = DateTimeOffset.Now;
        
        const string guestName = "Anonymity1";
        
        var user1 = await _accountUtil.AddUserAccount("greg", "123456", issuer: UserAccountIssuer.Guest).ConfigureAwait(false);
        var user2 = await _accountUtil.AddUserAccount("mars", "123456", issuer: UserAccountIssuer.Guest).ConfigureAwait(false);

        var meeting = await _meetingUtil.ScheduleMeeting();

        await _meetingUtil.JoinMeeting(meeting.Data.MeetingNumber);
        await _meetingUtil.JoinMeetingByUserAsync(user1, meeting.Data.MeetingNumber);
        await _meetingUtil.JoinMeetingByUserAsync(user2, meeting.Data.MeetingNumber);

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            await repository.InsertAllAsync(new List<MeetingSpeech>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    MeetingId = meeting.Data.Id,
                    OriginalText = "text",
                    CreatedDate = now,
                    UserId = user1.Id
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    MeetingId = meeting.Data.Id,
                    OriginalText = "text2",
                    CreatedDate = now.AddHours(1),
                    UserId = user1.Id
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    MeetingId = meeting.Data.Id,
                    OriginalText = "text3",
                    UserId = user2.Id,
                    Status = SpeechStatus.Cancelled
                }
            }, CancellationToken.None);
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
    
            builder.RegisterInstance(openAiService);
        });

        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            var response = await mediator.RequestAsync<GetMeetingAudioListRequest, GetMeetingAudioListResponse>(
                new GetMeetingAudioListRequest
                {
                    MeetingId = meeting.Data.Id, 
                    LanguageType = SpeechTargetLanguageType.Cantonese,
                    FilterHasCanceledAudio = true
                });

            response.Data.Count.ShouldBe(2);
            
            response.Data.First().UserId.ShouldBe(user1.Id);
            response.Data.First().UserName.ShouldBe(guestName);
            response.Data.First().MeetingId.ShouldBe(meeting.Data.Id);
            response.Data.First().OriginalText.ShouldBe("text");
            response.Data.First().VoiceUrl.ShouldBe("test.url");
            response.Data.First().TranslatedText.ShouldBe("translated_text");
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            var speechClient = Substitute.For<ISpeechClient>();

            speechClient.TranslateTextAsync(Arg.Any<TextTranslationDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "translated_text" });
            
            speechClient.GetTextFromAudioAsync(Arg.Any<SpeechToTextDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "text" });
            
            speechClient.GetAudioFromTextAsync(Arg.Any<TextToSpeechDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "test.url" });

            builder.RegisterInstance(openAiService);
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
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
    
            builder.RegisterInstance(openAiService);
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
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
    
            builder.RegisterInstance(openAiService);
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
            var openAiService = Substitute.For<IOpenAiService>();
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(openAiService);
            builder.RegisterInstance(liveKitServerUtilService);
        });
    }

    [Theory]
    [InlineData(false, "我是ea产生的voiceUrl")]
    [InlineData(true, "我是系统音色产生的voiceUrl")]
    public async Task CanGetMeetingChatVoiceRecord(bool isSystem, string voiceUrl)
    {
        var meetingId = Guid.NewGuid();
        var now = DateTimeOffset.Now;
        var voiceId = isSystem ? "123" : Guid.NewGuid().ToString();

        var roomSetting = new MeetingChatRoomSetting()
        {
            Id = 1,
            MeetingId = meetingId,
            UserId = 1,
            IsSystem = isSystem,
            VoiceId = voiceId,
            VoiceName = "小李的voice",
            Transpose = 1,
            Speed = 2,
            Style = 1,
            SelfLanguage = SpeechTargetLanguageType.English,
            ListeningLanguage = SpeechTargetLanguageType.Cantonese
        };

        var meetingSpeech1 = new MeetingSpeech()
        {
            MeetingId = meetingId,
            Status = SpeechStatus.UnViewed,
            Id = Guid.NewGuid(),
            VoiceId = voiceId,
            OriginalText = "你好呀",
            UserId = 1,
            CreatedDate = DateTimeOffset.Now.AddSeconds(-1000)
        };

        var meetingSpeech2 = new MeetingSpeech()
        {
            MeetingId = meetingId,
            Status = SpeechStatus.UnViewed,
            Id = Guid.NewGuid(),
            VoiceId = voiceId,
            OriginalText = "我是小李呀",
            UserId = 1,
            CreatedDate = DateTimeOffset.Now
        };

        var meetingChatVoiceRecord1 = new MeetingChatVoiceRecord
        {
            Id = Guid.NewGuid(),
            IsSelf = true,
            GenerationStatus = ChatRecordGenerationStatus.Completed,
            VoiceId =  ,
            SpeechId = meetingSpeech1.Id,
            VoiceUrl = "test.url",
            VoiceLanguage = SpeechTargetLanguageType.Cantonese,
            CreatedDate = DateTimeOffset.Now.AddSeconds(-500)
        };
        
        var meetingUserSession = new MeetingUserSession
        {
            MeetingId = meetingId,
            UserId = 1
        };

        var meetingTable = new Meeting()
        {
            Id = meetingId,
            MeetingNumber = "99999",
            MeetingMasterUserId = 1,
            StartDate = now.ToUnixTimeMilliseconds(),
            EndDate = now.ToUnixTimeMilliseconds() + 3600,
            OriginAddress = "test",
        };

        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(meetingChatVoiceRecord1);
            await repository.InsertAsync(roomSetting);
            await repository.InsertAsync(meetingUserSession);
            await repository.InsertAsync(meetingTable);
            await repository.InsertAsync(meetingSpeech1);
            await repository.InsertAsync(meetingSpeech2);
        });
        
        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var response = await mediator.RequestAsync<GetMeetingChatVoiceRecordRequest, GetMeetingChatVoiceRecordResponse>(
                new GetMeetingChatVoiceRecordRequest 
                {
                    MeetingId = meetingId,
                    FilterHasCanceledAudio = false,
                });

            response.Data.Count.ShouldBe(2);
            response.Data.First().MeetingId.ShouldBe(meetingId);
            response.Data[0].OriginalText.ShouldBe("你好呀");
            response.Data[0].VoiceRecord.VoiceUrl.ShouldBe("test.url");

            var voiceRecord = await repository
                .QueryNoTracking<MeetingChatVoiceRecord>().ToListAsync();
            
            voiceRecord.Count.ShouldBe(2);
            voiceRecord[0].VoiceUrl.ShouldBe("test.url");
            voiceRecord[1].VoiceUrl.ShouldBe(voiceUrl);
        }, builder =>
        {
            var speechClient = Substitute.For<ISpeechClient>();
            var smartiesClient = Substitute.For<ISmartiesClient>();

            smartiesClient.GetEchoAvatarVoiceSettingAsync(Arg.Any<GetEchoAvatarVoiceSettingRequestDto>(), CancellationToken.None)
                .Returns(new GetEchoAvatarUserToneResponse
                {
                    Data = new EchoAvatarUserToneDto
                    {
                        InferenceRecords = new List<EchoAvatarInferenceRecordDto>
                        {
                           new ()
                           {
                               Language = EchoAvatarLanguageType.Cantonese,
                               Style = 1
                           }
                        }
                    }
                });

            speechClient.SpeechInferenceAsync(Arg.Any<SpeechInferenceDto>(), CancellationToken.None)
                .Returns(new SpeechInferenceResponseDto
                {
                    Result = new SpeechInferenceResultDto { Url = "我是ea产生的voiceUrl"}
                });
          
            speechClient.TranslateTextAsync(Arg.Any<TextTranslationDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "你好呀" });

            speechClient.GetAudioFromTextAsync(Arg.Any<TextToSpeechDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "我是系统音色产生的voiceUrl" });
            
            builder.RegisterInstance(speechClient);
            builder.RegisterInstance(smartiesClient);
        });
    }
}