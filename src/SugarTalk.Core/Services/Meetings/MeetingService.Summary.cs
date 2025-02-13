using System;
using Serilog;
using System.Linq;
using Mediator.Net;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Constants;
using System.Collections.Generic;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Dto.Translation;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Core.Services.Meetings.Exceptions;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Dto.Meetings.Summary;
using SugarTalk.Messages.Enums.Meeting.Summary;
using SugarTalk.Messages.Events.Meeting.Summary;
using SugarTalk.Messages.Commands.Meetings.Summary;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<MeetingRecordSummarizedEvent> SummaryMeetingRecordAsync(SummaryMeetingRecordCommand command, CancellationToken cancellationToken);

    Task SummarizeMeetingInTargetLanguageAsync(ProcessSummaryMeetingCommand command, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<MeetingRecordSummarizedEvent> SummaryMeetingRecordAsync(
        SummaryMeetingRecordCommand command, CancellationToken cancellationToken)
    {
        var speakIds = string.Join(",", command.SpeakInfos.OrderBy(x => x.SpeakTime).Select(x => x.Id));
        
        var summary = (await _meetingDataProvider.GetMeetingSummariesAsync(
            recordId: command.MeetingRecordId,
            speakIds: speakIds,
            language: command.Language,
            status: SummaryStatus.Completed,
            cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        
        Log.Information("History summary: {@Summary}", summary);

        if (summary != null)
            return new MeetingRecordSummarizedEvent
            {
                Summary = _mapper.Map<MeetingSummaryDto>(summary)
            };
        
        summary = new MeetingSummary
        {
            SpeakIds = speakIds,
            Status = SummaryStatus.InProgress,
            RecordId = command.MeetingRecordId,
            MeetingNumber = command.MeetingNumber,
            TargetLanguage = command.Language,
            OriginText = GenerateOriginRecordText(command.SpeakInfos)
        };

        var record = (await _meetingDataProvider.GetMeetingRecordsAsync(command.MeetingRecordId, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        
        Log.Information("Get record for summary: {@Record}", record);

        await _meetingDataProvider.AddMeetingSummariesAsync(new List<MeetingSummary> { summary }, cancellationToken: cancellationToken).ConfigureAwait(false);

        _backgroundJobClient.Enqueue<IMediator>(x => x.SendAsync(new ProcessSummaryMeetingCommand
        {
            MeetingSummaryId = summary.Id,
            Language = command.Language
        }, cancellationToken), HangfireConstants.MeetingSummaryQueue);
        
        return new MeetingRecordSummarizedEvent
        {
            Summary = _mapper.Map<MeetingSummaryDto>(summary)
        };
    }

    public async Task SummarizeMeetingInTargetLanguageAsync(ProcessSummaryMeetingCommand command, CancellationToken cancellationToken)
    {
        var summary = (await _meetingDataProvider
            .GetMeetingSummariesAsync(command.MeetingSummaryId, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (summary == null) throw new MissingSummaryMeetingException();
        
        await SafelySummarizeAsync(summary, async () =>
        {
            var summaryInfo = await _meetingDataProvider
                .GetMeetingSummaryBaseBySummaryIdInfoAsync(summary.Id, cancellationToken).ConfigureAwait(false);
            
            Log.Information("Process summary : {@Summary}, summary info: {@SummaryInfo}", summary, summaryInfo);
            
            var originSummary = await _meetingUtilService.SummarizeAsync(summaryInfo, cancellationToken).ConfigureAwait(false);

            Log.Information("Generate origin summary: {OriginSummary}", originSummary);
            
            summary.Summary = (await _speechClient.TranslateTextAsync(new TextTranslationDto
            {
                Text = originSummary,
                TargetLanguageType = command.Language switch
                {
                    TranslationLanguage.ZhCn => SpeechTargetLanguageType.Mandarin,
                    TranslationLanguage.EnUs => SpeechTargetLanguageType.English,
                    TranslationLanguage.EsEs => SpeechTargetLanguageType.Spanish,
                    TranslationLanguage.KoKr => SpeechTargetLanguageType.Korean,
                    _ => SpeechTargetLanguageType.Mandarin
                },
            },  cancellationToken: cancellationToken).ConfigureAwait(false)).Result;
            
            Log.Information("Translated summary: {Summary}", summary.Summary);
            
            CheckMeetingSummarized(summary);
            
            await MarkRecordSummaryAsSpecifiedStatusAsync(summary, SummaryStatus.Completed, cancellationToken).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
    }
    
    public static string GenerateOriginRecordText(List<MeetingSpeakInfoDto> speakInfos)
    {
        var originText = speakInfos.OrderBy(x => x.SpeakTime)
            .Select(speakInfo => $"<{speakInfo.UserName}> ({DateTimeOffset.FromUnixTimeSeconds(speakInfo.SpeakTime):yyyy-MM-dd HH:mm:ss}) : {speakInfo.SpeakContent}")
            .Aggregate((current, next) => current + "\n" + next);
        
        Log.Information("Generating origin record text for summary: {OriginText}", originText);

        return originText;
    }
    
    private async Task MarkRecordSummaryAsSpecifiedStatusAsync(
        MeetingSummary summary, SummaryStatus status, CancellationToken cancellationToken)
    {
        summary.Status = status;

        await _meetingDataProvider.UpdateMeetingSummariesAsync(
            new List<MeetingSummary> { summary }, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
    
    private async Task SafelySummarizeAsync(MeetingSummary summary, Func<Task> action, CancellationToken cancellationToken)
    {
        var shouldReSummarize = false;
        
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            Log.Information("Summarizing {@Summary}", summary);
            
            await action().ConfigureAwait(false);
            
            Log.Information("Summarized {@Summary}", summary);

            if (string.IsNullOrEmpty(summary.Summary)) shouldReSummarize = true;
        }
        catch (Exception ex)
        {
            shouldReSummarize = true;
            
            Log.Information(ex, "Error on summarizing {@SummaryRecord}", summary);
        }

        if (shouldReSummarize)
        {
            await MarkRecordSummaryAsSpecifiedStatusAsync(summary, SummaryStatus.Pending, cancellationToken).ConfigureAwait(false);
            
            Log.Information("ReSummarizing {@SummaryRecord}", summary);
        }
    }
    
    private static void CheckMeetingSummarized(MeetingSummary summary)
    {
        if (string.IsNullOrEmpty(summary?.Summary)) throw new FailedMeetingSummaryException();
    }
}