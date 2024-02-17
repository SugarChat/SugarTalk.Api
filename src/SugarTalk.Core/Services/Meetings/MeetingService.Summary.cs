using System;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Dto.Meetings.Summary;
using SugarTalk.Messages.Enums.Meeting.Summary;
using SugarTalk.Messages.Events.Meeting.Summary;
using SugarTalk.Messages.Commands.Meetings.Summary;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<MeetingRecordSummarizedEvent> SummaryMeetingRecordAsync(SummaryMeetingRecordCommand command, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<MeetingRecordSummarizedEvent> SummaryMeetingRecordAsync(
        SummaryMeetingRecordCommand command, CancellationToken cancellationToken)
    {
        var speakIds = string.Join(",", command.SpeakInfos.OrderBy(x => x.SpeakTime).Select(x => x.Id));
        
        var summaries = await _meetingDataProvider.GetMeetingSummariesAsync(
            recordId: command.MeetingRecordId,
            speakIds: speakIds,
            language: command.Language,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        
        Log.Information("History summary: {@Task}", summaries);

        if (summaries != null && summaries.Any())
            return new MeetingRecordSummarizedEvent
            {
                Summary = _mapper.Map<MeetingSummaryDto>(summaries.FirstOrDefault())
            };
        
        var summary = new MeetingSummary
        {
            SpeakIds = speakIds,
            Status = SummaryStatus.InProgress,
            RecordId = command.MeetingRecordId,
            MeetingNumber = command.MeetingNumber,
            OriginText = GenerateOriginRecordText(command.SpeakInfos)
        };

        var record = (await _meetingDataProvider.GetMeetingRecordsAsync(command.MeetingRecordId, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        
        Log.Information("Get record for summary: {@Record}", record);

        await _meetingDataProvider.AddMeetingSummariesAsync(new List<MeetingSummary> { summary }, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        return new MeetingRecordSummarizedEvent
        {
            Summary = _mapper.Map<MeetingSummaryDto>(summary)
        };
    }

    public static string GenerateOriginRecordText(List<MeetingSpeakInfoDto> speakInfos)
    {
        var originText = speakInfos.OrderBy(x => x.SpeakTime)
            .Select(speakInfo => $"<{speakInfo.UserName}> ({DateTimeOffset.FromUnixTimeSeconds(speakInfo.SpeakTime):yyyy-MM-dd HH:mm:ss}) : {speakInfo.SpeakContent}")
            .Aggregate((current, next) => current + "\n" + next);
        
        Log.Information("Generating origin record text for summary: {OriginText}", originText);

        return originText;
    }

    public async Task SummarizeMeetingInTargetLanguageAsync(ProcessSummaryMeetingCommand command, CancellationToken cancellationToken)
    {
        var summary = (await _meetingDataProvider
            .GetMeetingSummariesAsync(command.SummaryInfo.MeetingSummaryId, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (summary == null) throw new Exception();
        
        await SafelySummarizeAsync(summary, async () =>
        {
            Log.Information("Process summary : {@Summary}", summary);
            
            summary.Summary = await _meetingUtilService.SummarizeAsync(command.SummaryInfo, cancellationToken).ConfigureAwait(false);
            
            CheckMeetingSummarized(summary);
            
            await MarkRecordSummaryAsSpecifiedStatusAsync(summary, SummaryStatus.Completed, cancellationToken).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
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
        if (string.IsNullOrEmpty(summary?.Summary))
            throw new Exception();
    }
}