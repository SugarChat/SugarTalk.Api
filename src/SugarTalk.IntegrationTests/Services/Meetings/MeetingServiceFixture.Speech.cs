using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Mediator.Net;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Requests.Speech;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Fact]
    public async Task ShouldSaveMeetingSpeech()
    {
        var currentUser = new TestCurrentUser();
        
        var command = new SaveMeetingAudioCommand
        {
            MeetingId = Guid.NewGuid(),
            AudioForBase64 = "巴卡巴卡巴卡",
            TargetLanguageType = SpeechTargetLanguageType.Cantonese
        };
        
        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            await mediator.SendAsync(command);

            var meetingSpeech = await repository.QueryNoTracking<MeetingSpeech>().SingleAsync(CancellationToken.None);
            
            meetingSpeech.MeetingId.ShouldBe(command.MeetingId);
            meetingSpeech.OriginalText.ShouldBe("text");
            meetingSpeech.TranslatedText.ShouldBe("translated_text");
            meetingSpeech.UserId.ShouldBe(currentUser.Id.Value);
        }, builder =>
        {
            var speechClient = Substitute.For<ISpeechClient>();

            speechClient.TranslateTextAsync(Arg.Any<TextTranslationDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "translated_text" });
            
            speechClient.GetTextFromAudioAsync(Arg.Any<SpeechToTextDto>(), CancellationToken.None)
                .Returns(new SpeechResponseDto { Result = "text" });

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
                    TranslatedText = "translated_text",
                    CreatedDate = now,
                    UserId = user1.Id
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    MeetingId = meetingId,
                    OriginalText = "text2",
                    TranslatedText = "translated_text2",
                    CreatedDate = now.AddHours(1),
                    UserId = user2.Id
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    MeetingId = meetingId,
                    OriginalText = "text3",
                    TranslatedText = "translated_text3",
                    UserId = user2.Id,
                    Status = SpeechStatus.Cancelled
                }
            }, CancellationToken.None);
        });

        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            var response = await mediator.RequestAsync<GetMeetingAudioListRequest, GetMeetingAudioListResponse>(
                new GetMeetingAudioListRequest { MeetingId = meetingId, FilterHasCanceledAudio = true });

            response.Data.Count.ShouldBe(2);
            
            response.Data.First().UserId.ShouldBe(user1.Id);
            response.Data.First().UserName.ShouldBe(user1.UserName);
            response.Data.First().MeetingId.ShouldBe(meetingId);
            response.Data.First().OriginalText.ShouldBe("text");
            response.Data.First().TranslatedText.ShouldBe("translated_text");
        });
    }
}