using System;
using Serilog;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Messages.Dto.OpenAi;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;
using SugarTalk.Messages.Dto.Smarties;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Dto.Meetings.Summary;

namespace SugarTalk.Core.Services.Meetings;

public interface IMeetingUtilService : IScopedDependency
{
    Task<string> SummarizeAsync(
        MeetingSummaryBaseInfoDto summaryBaseInfo, CancellationToken cancellationToken);
}

public class MeetingUtilService : IMeetingUtilService
{
    private readonly IOpenAiService _openAiService;
    private readonly ISmartiesClient _smartiesClient;

    public MeetingUtilService(IOpenAiService openAiService, ISmartiesClient smartiesClient)
    {
        _openAiService = openAiService;
        _smartiesClient = smartiesClient;
    }

    public async Task<string> SummarizeAsync(
        MeetingSummaryBaseInfoDto summaryBaseInfo, CancellationToken cancellationToken)
    {
        const int maxRecordLength = 10000;
        const int maxSummarySegmentsCount = 6;
        
        var summaries = new List<string>();
        var splitSummaries = new List<string>();
        
        var (dividedRecords, integrationCount) = SplitOriginalRecord(summaryBaseInfo.MeetingRecord, maxRecordLength, maxSummarySegmentsCount);
        
        switch (integrationCount)
        {
            case 0:
                return await SummarizeSingleMeetingAsync(summaryBaseInfo, dividedRecords.Single(), cancellationToken).ConfigureAwait(false);
            case 1:
                foreach (var dividedRecord in dividedRecords)
                {
                    var splitSummary = await SummarizeSingleMeetingAsync(summaryBaseInfo, dividedRecord, cancellationToken).ConfigureAwait(false);
                    
                    splitSummaries.Add(splitSummary);
                }
                break;
            case > 1:
                foreach (var dividedRecord in dividedRecords)
                {
                    var splitSummary = await SummarizeSingleMeetingAsync(summaryBaseInfo, dividedRecord, cancellationToken).ConfigureAwait(false);
                    
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
        MeetingSummaryBaseInfoDto summaryBaseInfo, string originalRecord, CancellationToken cancellationToken)
    {
        var summaryContent = await SummarizeMeetingContentAsync(originalRecord, cancellationToken).ConfigureAwait(false);

        var todo = await SummarizeMeetingTodoAsync(originalRecord, cancellationToken).ConfigureAwait(false);

        var meetingSummary = new MeetingSummaryJsonDto
        {   
            Abstract = string.IsNullOrEmpty(summaryContent) ? null : JsonConvert.DeserializeObject<List<MeetingAbstractDto>>(summaryContent),
            MeetingTodoItems = string.IsNullOrEmpty(todo) ? null : JsonConvert.DeserializeObject<List<MeetingTodoItemsDto>>(todo)
        };

        return JsonConvert.SerializeObject(meetingSummary);
    }
    
    private async Task<string> SummarizeMeetingContentAsync(string originalRecord, CancellationToken cancellationToken)
    {
        var messages = new List<CompletionsRequestMessageDto>
        {
            new ()
            {
                Role = "system",
                Content = "You are a trained AI that excels at language understanding and conference summaries. I want you to read the text below and divide it into several necessary summary paragraphs. Strive to retain the most important points and provide a coherent and easy-to-read summary that helps people understand the main points of the discussion without having to read the entire text. Please avoid unnecessary details or digressions. Please use the following json template to generate traditional Chinese fills. The objects in the json template generate the corresponding number according to the summary paragraph: \n[{\"abstract_title\":\"xxx\",\"abstract_content\":\"xxx\"},{\"abstract_title\":\"xxx\",\"abstract_content\":\"xxx\"},{\"abstract_title\":\"xxx\",\"abstract_content\":\"xxx\"}]"
            },
            new ()
            {
                Role = "user",
                Content = $" minutes:\n\"{originalRecord}\"\nmeeting summary:\n"
            }
        };
        
        var summary = await _smartiesClient.PerformQueryAsync(new AskGptRequestDto
            {
                Model = OpenAiModel.Gpt40Turbo,
                Messages = messages
            }, cancellationToken).ConfigureAwait(false);
        
        Log.Information("OriginRecord: {OriginRecord},\n SummaryContent: {Summary}", originalRecord, summary);

        return summary.Data.Response ?? string.Empty;
    }
    
    private async Task<string> SummarizeMeetingTodoAsync(string originalRecord, CancellationToken cancellationToken)
    {
        var messages = new List<CompletionsRequestMessageDto>
        {
            new ()
            {
                Role = "system",
                Content = "You are an AI expert at analyzing conversations and extracting action items. Please read the text and identify any tasks, assignments, or actions that have been agreed upon or mentioned as needing to be completed. These may be tasks assigned to specific individuals or general actions that the group has decided to take. Please list these action items clearly and concisely point by point in the following format: [{\"meeting_todo_item\": \"xxx\"},{\"meeting_todo_item\": \"xxx\"},{\"meeting_todo_item\": \"xxx\"}]. Must be listed in Traditional Chinese without sequence numbers"
            },
            new ()
            {
                Role = "user",
                Content = $" minutes:\n\"{originalRecord}\"\nmeeting summary:\n"
            }
        };
        
        var todo = await _smartiesClient.PerformQueryAsync(new AskGptRequestDto
        {
            Model = OpenAiModel.Gpt40Turbo,
            Messages = messages
        }, cancellationToken).ConfigureAwait(false);
        
        
        Log.Information("OriginRecord: {OriginRecord},\n Todo: {Todo}", originalRecord, todo);

        return todo.Data.Response ?? string.Empty;
    }
    
    private async Task<string> SummarizeMeetingSummaryAsync(string originalRecord, CancellationToken cancellationToken)
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

        /*var summary = 
            await _openAiService.ChatCompletionsAsync(messages, model: OpenAiModel.Gpt40Turbo, cancellationToken: cancellationToken).ConfigureAwait(false);*/
        
        var summary = await _smartiesClient.PerformQueryAsync(new AskGptRequestDto
        {
            Model = OpenAiModel.Gpt40Turbo,
            Messages = messages
        }, cancellationToken).ConfigureAwait(false);
        
        Log.Information("OriginRecord: {OriginRecord},\n Summary: {Summary}", originalRecord, summary);

        return summary.Data.Response ?? string.Empty;
    }
    
    private async Task<string> IntegrationMeetingSummariesAsync(IEnumerable<string> summaries, CancellationToken cancellationToken)
    {
        var concatenatedSummaries = string.Join("\n", summaries.Select((summary, index) => $"Part {index + 1} meeting summary:\n\"{summary}\""));
        
        var messages = new List<CompletionsRequestMessageDto>
        {
            new ()
            {
                Role = "system",
                Content =  "You are a conference summary content integration assistant. Please help me make multiple conference summaries into a logical content integration, " +
                           " and return the complete conference summary in strict accordance with the format, the format is as follows:\n" +
                           "\"{\"meeting_summary\":\"Conference Summary\",\"Abstract\":[{\"abstract_title\":\"xxx\",\"abstract_content\":\"xxx\"},{\"abstract_title\":\"xxx\",\"abstract_content\":\"xxx\"}],\"meeting_todo\":\"Conference To Do\",\"meeting_todo_items\":[{\"meeting_todo_item\":\"xxx\"},{\"meeting_todo_item\":\"xxx\"}]}\""
            },
            new ()
            {
                Role = "user",
                Content = $"{concatenatedSummaries}\nafter integration:\n"
            }
        };

        /*var integratedResponse = 
            await _openAiService.ChatCompletionsAsync(messages, model: OpenAiModel.Gpt40Turbo, cancellationToken: cancellationToken).ConfigureAwait(false);*/
        
        var integratedResponse = await _smartiesClient.PerformQueryAsync(new AskGptRequestDto
        {
            Model = OpenAiModel.Gpt40Turbo,
            Messages = messages
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("Summaries: {Summaries},\n IntegrationSummary: {IntegrationSummary}", concatenatedSummaries, integratedResponse);

        return integratedResponse.Data.Response ?? string.Empty;
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