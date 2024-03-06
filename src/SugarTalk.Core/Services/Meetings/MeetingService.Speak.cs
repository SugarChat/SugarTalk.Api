using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Enums.Meeting.Speak;
using SugarTalk.Messages.Events.Meeting.Speak;
using SugarTalk.Messages.Commands.Meetings.Speak;
using SugarTalk.Core.Services.Meetings.Exceptions;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<MeetingSpeakRecordedEvent> RecordMeetingSpeakAsync(RecordMeetingSpeakCommand command, CancellationToken cancellationToken);
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

    private async Task TranscriptionMeetingAsync(MeetingSpeakDetail speakDetail, MeetingRecord meetingRecord, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(meetingRecord.Url)) speakDetail.FileTranscriptionStatus = FileTranscriptionStatus.Pending;
        
        await _meetingDataProvider.UpdateMeetingSpeakDetailAsync(speakDetail, true, cancellationToken).ConfigureAwait(false);
        
        var recordFile = await _openAiService.GetAsync<byte[]>(meetingRecord.Url, cancellationToken).ConfigureAwait(false);

        var speakStartTimeVideo = speakDetail.SpeakStartTime - meetingRecord.CreatedDate.ToUnixTimeMilliseconds();
        var speakEndTimeVideo = (speakDetail.SpeakEndTime ?? 0) - meetingRecord.CreatedDate.ToUnixTimeMilliseconds();

        Log.Information("Start time of speak in video: {SpeakStartTimeVideo}, End time of speak in video: {SpeakEndTimeVideo}", speakStartTimeVideo, speakEndTimeVideo);
        
        try
        {
            speakDetail.SpeakContent = await _openAiService.TranscriptionAsync(
                recordFile, TranscriptionLanguage.Chinese, speakStartTimeVideo, speakEndTimeVideo,
                TranscriptionFileType.Mp4, cancellationToken: cancellationToken).ConfigureAwait(false);

            speakDetail.FileTranscriptionStatus = FileTranscriptionStatus.Completed;
        }
        catch (Exception ex)
        {
            speakDetail.FileTranscriptionStatus = FileTranscriptionStatus.Exception;
            
            Log.Information("transcription error: {ErrorMessage}", ex.Message);
        }
        finally
        {
            await _meetingDataProvider.UpdateMeetingSpeakDetailAsync(speakDetail, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}