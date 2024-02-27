using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Extensions;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Enums.Meeting.Speak;
using SugarTalk.Messages.Events.Meeting.Speak;
using SugarTalk.Messages.Commands.Meetings.Speak;
using SugarTalk.Core.Services.Meetings.Exceptions;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<MeetingSpeakRecordedEvent> RecordMeetingSpeakAsync(RecordMeetingSpeakCommand command, CancellationToken cancellationToken);

    Task UpdateMeetingFileTranscriptionStatusAsync(UpdateMeetingFileTranscriptionStatusCommand command, CancellationToken cancellationToken);
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

    public async Task UpdateMeetingFileTranscriptionStatusAsync(
        UpdateMeetingFileTranscriptionStatusCommand command, CancellationToken cancellationToken)
    {
        var meetingSpeakDetails = await _meetingDataProvider
            .GetMeetingSpeakDetailsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        
        var groupedMeetings = meetingSpeakDetails.Where(x =>
                x.FileTranscriptionStatus is FileTranscriptionStatus.Pending or FileTranscriptionStatus.InProcess)
            .GroupBy(x => x.MeetingNumber);

        foreach (var y in groupedMeetings.SelectMany(x => x))
        {
          await ProcessMeetingSpeakDetailAsync(y, cancellationToken).ConfigureAwait(false);
        }
    }
    
    private async Task ProcessMeetingSpeakDetailAsync(
        MeetingSpeakDetail meetingSpeakDetail, CancellationToken cancellationToken)
    {
       var getEgressInfoListResponse = await _liveKitClient.GetEgressInfoListAsync(
           new GetEgressRequestDto()
           {
               EgressId = meetingSpeakDetail.EgressId,
               Token = _liveKitServerUtilService.GenerateTokenForRecordMeeting(new UserAccountDto() {Id = 0, UserName = "user"}, meetingSpeakDetail.MeetingNumber)
           }, cancellationToken).ConfigureAwait(false);
       
       var egressItem = getEgressInfoListResponse?.EgressItems?.FirstOrDefault();

       if (egressItem != null)
       {
           meetingSpeakDetail.FileTranscriptionStatus = egressItem.Status switch
           {
               "EGRESS_STARTING" or "EGRESS_ACTIVE" or "EGRESS_ENDING" =>  FileTranscriptionStatus.Pending,
               "EGRESS_COMPLETE" or "EGRESS_LIMIT_REACHED" when !string.IsNullOrEmpty(egressItem.File.Location) => FileTranscriptionStatus.InProcess,
               _ => FileTranscriptionStatus.Exception
           };
           
           await _meetingDataProvider.UpdateMeetingSpeakDetailAsync(meetingSpeakDetail, cancellationToken: cancellationToken).ConfigureAwait(false);
           
           if (egressItem.Status is "EGRESS_COMPLETE" or "EGRESS_LIMIT_REACHED" && meetingSpeakDetail.FileTranscriptionStatus == FileTranscriptionStatus.InProcess)
               _backgroundJobClient.Enqueue(() => TranscriptionMeetingAsync(meetingSpeakDetail, cancellationToken));
       }
    }

    private async Task<MeetingSpeakDetail> StartRecordUserSpeakDetailAsync(RecordMeetingSpeakCommand command, CancellationToken cancellationToken)
    {
        var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        var token = _liveKitServerUtilService.GenerateTokenForRecordMeeting(user, command.MeetingNumber);
        var filePath = $"SugarTalk/{command.MeetingRecordId}.mp4";
        
        var egressResponse = await _liveKitClient.StartTrackEgressAsync(new StartTrackEgressRequestDto()
        {
            Token = token,
            TrackId = command.TrackId,
            RoomName = command.MeetingNumber,
            File = new EgressEncodedFileOutPutDto
            {
                FilePath = filePath,
                AliOssUpload = new EgressAliOssUploadDto
                {
                    AccessKey = _aliYunOssSetting.AccessKeyId,
                    Secret = _aliYunOssSetting.AccessKeySecret,
                    Bucket = _aliYunOssSetting.BucketName,
                    Endpoint = _aliYunOssSetting.Endpoint
                }
            }
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("Start track egress response: {@EgressResponse}", egressResponse);

        if (egressResponse == null) throw new Exception();
        
        var speakDetail = new MeetingSpeakDetail
        {
            FilePath = filePath,
            TrackId = command.TrackId,
            UserId = _currentUser.Id.Value,
            EgressId = egressResponse.EgressId,
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
        
        var egressResponse = await _liveKitClient.StopEgressAsync(new StopEgressRequestDto
        {
            EgressId = speakDetail.EgressId
        }, cancellationToken).ConfigureAwait(false);
        
        Log.Information("Stop egress response: {@EgressResponse}", egressResponse);

        if (egressResponse == null) throw new Exception();
        
        speakDetail.SpeakStatus = SpeakStatus.End;
        speakDetail.SpeakEndTime = command.SpeakEndTime.Value;
        
        await _meetingDataProvider.UpdateMeetingSpeakDetailAsync(speakDetail, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        if (speakDetail.FileTranscriptionStatus == FileTranscriptionStatus.Pending)
        {
            speakDetail.FileTranscriptionStatus = FileTranscriptionStatus.InProcess;
            
            await _meetingDataProvider.UpdateMeetingSpeakDetailAsync(speakDetail, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            _backgroundJobClient.Enqueue(() => TranscriptionMeetingAsync(speakDetail, cancellationToken));
        }
        
        return speakDetail;
    }

    private async Task TranscriptionMeetingAsync(MeetingSpeakDetail speakDetail,CancellationToken cancellationToken)
    {
        var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        var getEgressInfoList = await _liveKitClient.GetEgressInfoListAsync(
            new GetEgressRequestDto()
        {
            Token =  _liveKitServerUtilService.GenerateTokenForRecordMeeting(user, speakDetail.MeetingNumber),
            EgressId = speakDetail.EgressId
        }, cancellationToken).ConfigureAwait(false);

        speakDetail.FileUrl =  getEgressInfoList.EgressItems.FirstOrDefault()?.File.Location;

        if (string.IsNullOrEmpty(speakDetail.FileUrl)) speakDetail.FileTranscriptionStatus = FileTranscriptionStatus.Pending;

        var fileBytes = await _openAiService.GetAsync<byte[]>(speakDetail.FileUrl, cancellationToken).ConfigureAwait(false);
        
        speakDetail.SpeakContent = await _openAiService.TranscriptionAsync(fileBytes, TranscriptionLanguage.Chinese, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        if (speakDetail.SpeakContent == null) speakDetail.FileTranscriptionStatus = FileTranscriptionStatus.Exception;

        speakDetail.FileTranscriptionStatus = FileTranscriptionStatus.Completed;
        
        await _meetingDataProvider.UpdateMeetingSpeakDetailAsync(speakDetail, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}