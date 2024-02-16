using System;
using Serilog;
using System.Linq;
using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Messages.Dto.OpenAi;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.Core.Services.Meetings;

public interface IMeetingUtilService : IScopedDependency
{
    Task<string> SummarizeAsync(
        string meetingNumber, string meetingTitle, string meetingRecord, string meetingAdmin, string attendees, DateTimeOffset meetingDate, CancellationToken cancellationToken);
}

public class MeetingUtilService : IMeetingUtilService
{
    private readonly IOpenAiService _openAiService;

    public MeetingUtilService(IOpenAiService openAiService)
    {
        _openAiService = openAiService;
    }

    public async Task<string> SummarizeAsync(
        string meetingNumber, string meetingTitle, string meetingRecord, string meetingAdmin, string attendees, DateTimeOffset meetingDate, CancellationToken cancellationToken)
    {
        const int maxRecordLength = 10000;
        const int maxSummarySegmentsCount = 6;
        
        var summaries = new List<string>();
        var splitSummaries = new List<string>();
        
        var (dividedRecords, integrationCount) = SplitOriginalRecord(meetingRecord, maxRecordLength, maxSummarySegmentsCount);
        
        switch (integrationCount)
        {
            case 0:
                return await SummarizeSingleMeetingAsync(meetingNumber, meetingTitle, dividedRecords.Single(), meetingAdmin, attendees, meetingDate, cancellationToken).ConfigureAwait(false);
            case 1:
                foreach (var dividedRecord in dividedRecords)
                {
                    var splitSummary = await SummarizeSingleMeetingAsync(meetingNumber, meetingTitle, dividedRecord, meetingAdmin, attendees, meetingDate, cancellationToken).ConfigureAwait(false);
                    
                    splitSummaries.Add(splitSummary);
                }
                break;
            case > 1:
                foreach (var dividedRecord in dividedRecords)
                {
                    var splitSummary = await SummarizeSingleMeetingAsync(meetingNumber, meetingTitle, dividedRecord, meetingAdmin, attendees, meetingDate, cancellationToken).ConfigureAwait(false);
                    
                    splitSummaries.Add(splitSummary);

                    if (splitSummaries.Count != maxSummarySegmentsCount) continue;
                    
                    var summary = await IntegrationMeetingSummariesAsync(splitSummaries, cancellationToken).ConfigureAwait(false);
                    
                    summaries.Add(summary);
                    
                    splitSummaries.Clear();
                }
                break;
        }
        
        summaries.AddRange(splitSummaries);
        
        return await IntegrationMeetingSummariesAsync(summaries, cancellationToken).ConfigureAwait(false);
    }
    
    private async Task<string> SummarizeSingleMeetingAsync(
        string meetingNumber, string meetingTitle, string originalRecord, string meetingAdmin, string attendees, DateTimeOffset meetingDate, CancellationToken cancellationToken)
    {
        var summaryContent = await SummarizeMeetingContentAsync(meetingNumber, originalRecord, cancellationToken).ConfigureAwait(false);

        var todo = await SummarizeMeetingTodoAsync(meetingNumber, originalRecord, cancellationToken).ConfigureAwait(false);

        var summary = await SummarizeMeetingSummaryAsync(meetingNumber, originalRecord, cancellationToken).ConfigureAwait(false);
        
        var result = $"會議總結：\n\n會議主題：{meetingTitle}\n日期：{meetingDate}\n主持人：{meetingAdmin}\n參會人員：{attendees}\n\n{summaryContent}\n\n{todo}\n\n{summary}";
        
        return result;
    }
    
    private async Task<string> SummarizeMeetingContentAsync(string meetingCode, string originalRecord, CancellationToken cancellationToken)
    {
        var messages = new List<CompletionsRequestMessageDto>
        {
            new ()
            {
                Role = "system",
                Content = "You are a highly skilled AI trained in language comprehension and summarization. I would like you to read the following text and summarize it into a concise abstract paragraph. Aim to retain the most important points, providing a coherent and readable summary that could help a person understand the main points of the discussion without needing to read the entire text. Please avoid unnecessary details or tangential points. Please use the following template to generate in traditional Chinese: \n會議內容：1.xxx\n2.xxx\n3.xxx\n"
            },
            new ()
            {
                Role = "user",
                Content = $" minutes:\n\"{originalRecord}\"\nmeeting summary:\n"
            }
        };

        var summary = 
            await _openAiService.ChatCompletionsAsync(messages, model: OpenAiModel.Gpt40Turbo, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        Log.Information("MeetingCode: {MeetingCode},\nOriginRecord: {OriginRecord},\n SummaryContent: {Summary}", meetingCode, originalRecord, summary);

        return summary.Response ?? string.Empty;
    }
    
    private async Task<string> SummarizeMeetingTodoAsync(string meetingCode, string originalRecord, CancellationToken cancellationToken)
    {
        var messages = new List<CompletionsRequestMessageDto>
        {
            new ()
            {
                Role = "system",
                Content = "You are an AI expert in analyzing conversations and extracting action items. Please review the text and identify any tasks, assignments, or actions that were agreed upon or mentioned as needing to be done. These could be tasks assigned to specific individuals, or general actions that the group has decided to take. Please list these action items point by point clearly and concisely in the following format: \nTodo:1.xxx\n2.xxx\n3.xxx. Must list in tradition Chinese"
            },
            new ()
            {
                Role = "user",
                Content = $" minutes:\n\"{originalRecord}\"\nmeeting summary:\n"
            }
        };

        var todo = 
            await _openAiService.ChatCompletionsAsync(messages, model: OpenAiModel.Gpt40Turbo, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        Log.Information("MeetingCode: {MeetingCode},\nOriginRecord: {OriginRecord},\n Todo: {Todo}", meetingCode, originalRecord, todo);

        return todo.Response ?? string.Empty;
    }
    
    private async Task<string> SummarizeMeetingSummaryAsync(string meetingCode, string originalRecord, CancellationToken cancellationToken)
    {
        var messages = new List<CompletionsRequestMessageDto>
        {
            new ()
            {
                Role = "system",
                Content = "You are a AI expert on the conference summary.Please review the text and summarize the meeting into one paragraph.Here is the summary format:總結:\\nxxx.Please answer in traditional Chinese."
            },
            new ()
            {
                Role = "user",
                Content = $" minutes:\n\"{originalRecord}\"\nmeeting summary:\n"
            }
        };

        var summary = 
            await _openAiService.ChatCompletionsAsync(messages, model: OpenAiModel.Gpt40Turbo, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        Log.Information("MeetingCode: {MeetingCode},\nOriginRecord: {OriginRecord},\n Summary: {Summary}", meetingCode, originalRecord, summary);

        return summary.Response ?? string.Empty;
    }
    
    private async Task<string> IntegrationMeetingSummariesAsync(IEnumerable<string> summaries, CancellationToken cancellationToken)
    {
        var concatenatedSummaries = string.Join("\n", summaries.Select((summary, index) => $"Part {index + 1} meeting summary:\n\"{summary}\""));
        
        var messages = new List<CompletionsRequestMessageDto>
        {
            new ()
            {
                Role = "system",
                Content =  "You are a meeting summary content integration assistant, please help me make multiple meeting summaries into a logical content integration, " +
                           "and return a complete meeting summary in the following format:\n" +
                           "\"會議總結：\n\n會議主題：xxx\n日期：xxx\n主持人:xxx\n參會人員: xxx,xxx\n會議內容：1.xxx\n2.xxx\n3.xxx\nTodo:1.xxx\n2.xxx\n3.xxx\n總結：xxx\""
            },
            new ()
            {
                Role = "user",
                Content = $"{concatenatedSummaries}\nafter integration:\n"
            }
        };

        var integratedResponse = 
            await _openAiService.ChatCompletionsAsync(messages, model: OpenAiModel.Gpt40Turbo, cancellationToken: cancellationToken).ConfigureAwait(false);

        Log.Information("Summaries: {Summaries},\n IntegrationSummary: {IntegrationSummary}", concatenatedSummaries, integratedResponse);

        return integratedResponse.Response ?? string.Empty;
    }
    
    public static (List<string> DividedRecords, int IntegrationCount) SplitOriginalRecord(string originalRecord, int maxRecordLength, int maxSummarySegmentsCount)
    {
        var dividedRecords = Enumerable.Range(0, (int)Math.Ceiling((double)originalRecord.Length / maxRecordLength))
            .Select(i => originalRecord.Substring(i * maxRecordLength, Math.Min(maxRecordLength, originalRecord.Length - i * maxRecordLength))).ToList();
        
        var integrationCount = dividedRecords.Count switch
        {
            1 => 0,
            var count and > 1 when count <= maxSummarySegmentsCount => 1,
            var count when count > maxSummarySegmentsCount => count / maxSummarySegmentsCount + 1,
            _ => 0
        };

        return (dividedRecords, integrationCount);
    }
}