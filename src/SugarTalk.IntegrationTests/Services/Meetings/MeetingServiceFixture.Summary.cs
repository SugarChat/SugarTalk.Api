using Xunit;
using System;
using Autofac;
using Shouldly;
using NSubstitute;
using System.Linq;
using Mediator.Net;
using System.Threading;
using SugarTalk.Core.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Cloud.Translation.V2;
using SugarTalk.Core.Services.Jobs;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.IntegrationTests.Mocks;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Dto.Meetings.Summary;
using SugarTalk.Messages.Enums.Meeting.Summary;
using SugarTalk.Messages.Commands.Meetings.Summary;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    public async Task ShouldSummaryMeetingRecord(bool existHistorySummary, bool canSummary, bool canTranslation)
    {
        var user = new UserAccount
        {
            Id = 2,
            UserName = "Monesy.H"
        };
        
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingMasterUserId = user.Id,
            MeetingNumber = "123456",
            Status = MeetingStatus.Completed,
            StartDate = 1706919857,
            EndDate = 1707006257
        };
        
        var record = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting.Id,
            Url = "http://www.baidu.com",
        };
        
        var summary = new MeetingSummary
        {
            Id = 1,
            RecordId = record.Id,
            MeetingNumber = meeting.MeetingNumber,
            Summary = "总结",
            SpeakIds = "1,2,3",
            OriginText = "<Monesy.H> (1970-01-01 00:00:00) : 你好\n<Bans.C> (1970-01-01 00:00:00) : 滚\n<Ohlinc.C> (1970-01-01 00:00:00) : 注意素质",
            Status = SummaryStatus.Completed
        };

        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            if (existHistorySummary)
                await repository.InsertAsync(summary).ConfigureAwait(false);

            await repository.InsertAsync(meeting).ConfigureAwait(false);

            await repository.InsertAsync(record).ConfigureAwait(false);
        });

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            await mediator.SendAsync(new SummaryMeetingRecordCommand
            {
                MeetingRecordId = record.Id,
                MeetingNumber = meeting.MeetingNumber,
                SpeakInfos = new List<MeetingSpeakInfoDto>
                {
                    new()
                    {
                        Id = 1,
                        UserName = "Monesy.H",
                        SpeakContent = "你好"
                    },
                    new()
                    {
                        Id = 2,
                        UserName = "Bans.C",
                        SpeakContent = "滚"
                    },
                    new()
                    {
                        Id = 3,
                        UserName = "Ohlinc.C",
                        SpeakContent = "注意素质"
                    }
                }
            }).ConfigureAwait(false);

            var meetingSummaries = await repository.Query<MeetingSummary>().ToListAsync().ConfigureAwait(false);
            
            meetingSummaries.ShouldNotBeEmpty();
            meetingSummaries.Count.ShouldBe(1);
            meetingSummaries.First().RecordId.ShouldBe(record.Id);
            meetingSummaries.First().SpeakIds.ShouldBe(summary.SpeakIds);
            meetingSummaries.First().MeetingNumber.ShouldBe(summary.MeetingNumber);
            meetingSummaries.First().OriginText.ShouldBe("<Monesy.H> (1970-01-01 00:00:00) : 你好\n<Bans.C> (1970-01-01 00:00:00) : 滚\n<Ohlinc.C> (1970-01-01 00:00:00) : 注意素质");

            if (canSummary && canTranslation || existHistorySummary)
            {
                meetingSummaries.First().Status.ShouldBe(SummaryStatus.Completed);   
            }
            else
                meetingSummaries.First().Status.ShouldBe(SummaryStatus.Pending);
        }, builder =>
        {
            var meetingUtilService = Substitute.For<IMeetingUtilService>();
            var openAiService = Substitute.For<IOpenAiService>();
            
            meetingUtilService.SummarizeAsync(Arg.Any<MeetingSummaryBaseInfoDto>(), Arg.Any<CancellationToken>())
                .Returns(canSummary ? "summary" : string.Empty);
            
            var translationClient = Substitute.For<TranslationClient>();

            translationClient.TranslateTextAsync(Arg.Is("summary"), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TranslationModel?>(), Arg.Any<CancellationToken>())
                .Returns(new TranslationResult("", canTranslation ? "总结" : "", "", "", "", null));

            builder.RegisterInstance(openAiService);
            builder.RegisterInstance(translationClient);
            builder.RegisterInstance(meetingUtilService);
            builder.RegisterType<MockingBackgroundJobClient>().As<ISugarTalkBackgroundJobClient>().InstancePerLifetimeScope();
        });
    }
}