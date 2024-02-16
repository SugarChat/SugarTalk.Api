using System;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Dto.Meetings.Summary;
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
        
        var tasks = await _meetingDataProvider.GetMeetingSummariesAsync(
            recordId: command.MeetingRecordId,
            speakIds: speakIds,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        
        Log.Information("History summary task: @{Task}", tasks);

        if (tasks != null && tasks.Any())
            return new MeetingRecordSummarizedEvent
            {
                Summary = _mapper.Map<MeetingSummaryDto>(tasks.FirstOrDefault())
            };
        
        var summary = new MeetingSummary
        {
            SpeakIds = speakIds,
            RecordId = command.MeetingRecordId,
            MeetingNumber = command.MeetingNumber,
            OriginText = GenerateOriginRecordText(command.SpeakInfos)
        };
        
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
}