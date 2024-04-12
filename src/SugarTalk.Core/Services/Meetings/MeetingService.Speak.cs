using System;
using System.Collections.Generic;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Enums.Meeting.Speak;
using SugarTalk.Messages.Events.Meeting.Speak;
using SugarTalk.Messages.Commands.Meetings.Speak;
using SugarTalk.Core.Services.Meetings.Exceptions;
using SugarTalk.Messages.Dto.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;
using CompletionsRequestMessageDto = SugarTalk.Messages.Dto.OpenAi.CompletionsRequestMessageDto;
using OpenAiModel = SugarTalk.Messages.Enums.OpenAi.OpenAiModel;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<MeetingSpeakRecordedEvent> RecordMeetingSpeakAsync(RecordMeetingSpeakCommand command, CancellationToken cancellationToken);

    Task OptimizeTranscribedContent(List<MeetingSpeakDetail> speakDetail, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<MeetingSpeakRecordedEvent> RecordMeetingSpeakAsync(RecordMeetingSpeakCommand command, CancellationToken cancellationToken)
    {
        var speakDetail = command.Id.HasValue switch
        {
            true => await EndRecordUserSpeakDetailAsync(command, cancellationToken).ConfigureAwait(false),
            false => await StartRecordUserSpeakDetailAsync(command, cancellationToken).ConfigureAwait(false)
        };

        var result = (await _meetingDataProvider.GetMeetingSpeakDetailsAsync(
            new List<int>{ speakDetail.Id }, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        
        return new MeetingSpeakRecordedEvent
        {
            MeetingSpeakDetail = _mapper.Map<MeetingSpeakDetailDto>(result)
        };
    }

    private async Task<MeetingSpeakDetail> StartRecordUserSpeakDetailAsync(RecordMeetingSpeakCommand command, CancellationToken cancellationToken)
    {
        var speakDetail = new MeetingSpeakDetail
        {
            TrackId = command.TrackId,
            Username = _currentUser.Name,
            UserId = _currentUser.Id.Value,
            MeetingNumber = command.MeetingNumber,
            MeetingRecordId = command.MeetingRecordId,
            SpeakStartTime = command.SpeakStartTime.Value
        };
        
        await _meetingDataProvider.AddMeetingSpeakDetailAsync(speakDetail, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        return speakDetail;
    }
    
    private async Task<MeetingSpeakDetail> EndRecordUserSpeakDetailAsync(RecordMeetingSpeakCommand command, CancellationToken cancellationToken)
    {
        var speakDetail = (await _meetingDataProvider.GetMeetingSpeakDetailsAsync(
            new List<int>{ command.Id.Value }, speakStatus: SpeakStatus.Speaking, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        Log.Information("Ending record user Speak, speak detail: {@SpeakDetail}", speakDetail);
        
        if (speakDetail == null) throw new SpeakNotFoundException();
        
        speakDetail.SpeakStatus = SpeakStatus.End;
        speakDetail.SpeakEndTime = command.SpeakEndTime.Value;
        
        await _meetingDataProvider.UpdateMeetingSpeakDetailAsync(speakDetail, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        return speakDetail;
    }
    
    private async Task TranscriptionMeetingAsync(List<MeetingSpeakDetail> speakDetails, MeetingRecord meetingRecord, CancellationToken cancellationToken)
    {
        foreach (var speakDetail in speakDetails)
        {
            var recordFile = await _httpClientFactory.GetAsync<byte[]>(meetingRecord.Url, cancellationToken).ConfigureAwait(false);

            var speakStartTimeVideo = speakDetail.SpeakStartTime - meetingRecord.CreatedDate.ToUnixTimeMilliseconds();
            var speakEndTimeVideo = (speakDetail.SpeakEndTime ?? 0) - meetingRecord.CreatedDate.ToUnixTimeMilliseconds();

            Log.Information("Start time of speak in video: {SpeakStartTimeVideo}, End time of speak in video: {SpeakEndTimeVideo}", speakStartTimeVideo, speakEndTimeVideo);
        
            try
            {
                speakDetail.OriginalContent = await _openAiService.TranscriptionAsync(
                    recordFile, TranscriptionLanguage.Chinese, speakStartTimeVideo, speakEndTimeVideo,
                    TranscriptionFileType.Mp4, TranscriptionResponseFormat.Text, cancellationToken: cancellationToken).ConfigureAwait(false);
                
                Log.Information("Complete transcribed content optimization" );
            
                speakDetail.FileTranscriptionStatus = FileTranscriptionStatus.Completed;
            }
            catch (Exception ex)
            {
                speakDetail.FileTranscriptionStatus = FileTranscriptionStatus.Exception;
            
                Log.Information("transcription error: {ErrorMessage}", ex.Message);
            }
        }
        
        await _meetingDataProvider.UpdateMeetingSpeakDetailsAsync(speakDetails, true, cancellationToken).ConfigureAwait(false);
        
        _backgroundJobClient.Enqueue<IMeetingService>(x => x.OptimizeTranscribedContent(speakDetails, cancellationToken));
    }

    public async Task OptimizeTranscribedContent(List<MeetingSpeakDetail> speakDetails, CancellationToken cancellationToken)
    {
        foreach (var speakDetail in speakDetails)
        {
            var message = new List<CompletionsRequestMessageDto>()
            {
                new()
                {
                    Role = "system",
                    Content = "你是一个会议智能助手，你可以概括含糊其辞的内容转化简单通俗易懂的句子." +
                              "你的目标是根据所获取到的内容提供更加清晰和易于理解的内容，必须保持原有意思不变。" +
                              "根据上下文和语句进行智能优化，进行纠正语句的语法，以至于段落信息变的更加通顺合理，但是不要在你获取的段落中额外添加其他信息," +
                              "你返回的内容必须是json格式,例如，当收到的输入为：我觉得emmm，这个吧，，，有点，，，我再想想，这时候你给我的返回必须是{\"optimized_text\": 这个问题我需要再想一下}",
                },

                new() { Role = "user", Content = $"输入: {speakDetail.OriginalContent}, 返回: " }
            };

            var openAiSmartContent = await _openAiService.ChatCompletionsAsync(
                message, model: OpenAiModel.Gpt35Turbo, responseFormat: new CompletionResponseFormatDto { Type = "json_object" }, cancellationToken: cancellationToken).ConfigureAwait(false);

            Log.Information("OriginalContent: {OriginalContent},\n SmartContent: {openAiSmartContent}", speakDetail.OriginalContent, openAiSmartContent.Response);

            speakDetail.SmartContent = JsonConvert.DeserializeObject<MeetingDetailSmartContentDto>(openAiSmartContent.Response).OptimizedText;

            Log.Information("SmartContent: {SmartContent}", speakDetail.SmartContent);

            await _meetingDataProvider.UpdateMeetingSpeakDetailAsync(speakDetail, true, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task MarkSpeakTranscriptAsSpecifiedStatusAsync(
        List<MeetingSpeakDetail> details, FileTranscriptionStatus status, CancellationToken cancellationToken)
    {
        details.ForEach(x => x.FileTranscriptionStatus = status);
        
        await _meetingDataProvider.UpdateMeetingSpeakDetailsAsync(details, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}