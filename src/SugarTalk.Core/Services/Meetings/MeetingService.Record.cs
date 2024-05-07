using System;
using Serilog;
using System.Linq;
using Mediator.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SugarTalk.Messages.Extensions;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Enums.Meeting.Speak;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Messages.Dto.Translation;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Requests.Meetings;
namespace SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Events.Meeting.Summary;

public partial interface IMeetingService
{
    Task<GetCurrentUserMeetingRecordResponse> GetCurrentUserMeetingRecordsAsync(GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken);
    
    Task<MeetingRecordDeletedEvent> DeleteMeetingRecordAsync(DeleteMeetingRecordCommand command, CancellationToken cancellationToken);

    Task<MeetingRecordingStartedEvent> StartMeetingRecordingAsync(StartMeetingRecordingCommand command, CancellationToken cancellationToken);
    
    Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(GetMeetingRecordDetailsRequest request, CancellationToken cancellationToken);
    
    Task<StorageMeetingRecordVideoResponse> StorageMeetingRecordVideoAsync(StorageMeetingRecordVideoCommand command, CancellationToken cancellationToken);
    
    Task<DelayedMeetingRecordingStorageEvent> ExecuteStorageMeetingRecordVideoDelayedJobAsync(DelayedMeetingRecordingStorageCommand command, CancellationToken cancellationToken);

    Task DelayStorageMeetingRecordVideoJobAsync(Guid meetingId,string egressId, Guid meetingRecordId, string token, int reTryLimit, bool isRestartRecording, CancellationToken cancellationToken);

    Task<GeneralOverRecordingEvent> GeneralRestartRecordAsync(GeneralRestartRecordCommand command, CancellationToken cancellationToken);
    
    Task<TranslatingMeetingSpeakResponse> TranslatingMeetingSpeakAsync(TranslatingMeetingSpeakCommand command, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<GetCurrentUserMeetingRecordResponse> GetCurrentUserMeetingRecordsAsync(GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken)
    {
        var (total, items) = await _meetingDataProvider.GetMeetingRecordsByUserIdAsync(_currentUser.Id, request, cancellationToken).ConfigureAwait(false);

        var response = new GetCurrentUserMeetingRecordResponse
        {
           Data = new GetCurrentUserMeetingRecordResponseDto
           {
               Count = total,
               Records = items
           }
        };

        return response;
    }

    public async Task<MeetingRecordingStartedEvent> StartMeetingRecordingAsync(StartMeetingRecordingCommand command, CancellationToken cancellationToken)
    {
        command.MeetingRecordId ??= Guid.NewGuid();
        
        var meeting = await _meetingDataProvider
            .GetMeetingByIdAsync(command.MeetingId, cancellationToken).ConfigureAwait(false);

        if (meeting is null) throw new MeetingNotFoundException();

        if (meeting.MeetingMasterUserId != _currentUser?.Id) 
            throw new CannotStartMeetingRecordingException(_currentUser?.Id);

        var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

        var postResponse = await _liveKitClient.StartRoomCompositeEgressAsync(new StartRoomCompositeEgressRequestDto
        {
            Token = _liveKitServerUtilService.GenerateTokenForRecordMeeting(user, meeting.MeetingNumber),
            RoomName = meeting.MeetingNumber,
            File = new EgressEncodedFileOutPutDto
            {
                FilePath = $"SugarTalk/{command.MeetingRecordId}.mp4",
                AliOssUpload = new EgressAliOssUploadDto
                {
                    AccessKey = _aliYunOssSetting.AccessKeyId,
                    Secret = _aliYunOssSetting.AccessKeySecret,
                    Bucket = _aliYunOssSetting.BucketName,
                    Endpoint = _aliYunOssSetting.Endpoint
                }
            }
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("start meeting recording response: {@postResponse}", postResponse);

        if (postResponse is null) throw new Exception("Start Meeting Recording Failed.");
        
        if (command.IsRestartRecord)
            await UpdateMeetingRecordEgressRestartAsync(command.MeetingRecordId.Value, postResponse.EgressId, cancellationToken).ConfigureAwait(false);
        else
            await AddMeetingRecordAsync(meeting, command.MeetingRecordId.Value, postResponse.EgressId, cancellationToken).ConfigureAwait(false);
      
        _sugarTalkBackgroundJobClient.Schedule<IMediator>(m => m.SendAsync(new GeneralRestartRecordCommand
        {
            MeetingId = meeting.Id,
            MeetingRecordId = command.MeetingRecordId.Value
        }, cancellationToken), TimeSpan.FromMinutes(30));
        
        return new MeetingRecordingStartedEvent
        {
            MeetingRecordId = command.MeetingRecordId.Value,
            EgressId = postResponse.EgressId
        };
    }

    private async Task AddMeetingRecordAsync(Meeting meeting, Guid meetingRecordId, string egressId, CancellationToken cancellationToken)
    {
        await _meetingDataProvider.PersistMeetingRecordAsync(meeting.Id, meetingRecordId, egressId, cancellationToken).ConfigureAwait(false);

        await _meetingDataProvider.AddMeetingRecordVoiceRelayStationAsync(new MeetingRestartRecord
        {
            Id = new Guid(),
            MeetingId = meeting.Id,
            RecordId = meetingRecordId
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        meeting.IsActiveRecord = true;

        await _meetingDataProvider.UpdateMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);
    }

    private async Task UpdateMeetingRecordEgressRestartAsync(Guid meetingRecordId, string egressId, CancellationToken cancellationToken)
    {
        var record = (await _meetingDataProvider.GetMeetingRecordsAsync(meetingRecordId, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        
        Log.Information("restart meeting recording response: {@record}", record);

        if (record != null)
        {
            record.EgressId = egressId;

            await _meetingDataProvider.UpdateMeetingRecordAsync(record, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<GeneralOverRecordingEvent> GeneralRestartRecordAsync(
        GeneralRestartRecordCommand command, CancellationToken cancellationToken)
    {
        Log.Information("GeneralRestartRecordCommand is: command {@command}",JsonConvert.SerializeObject(command));
        
        var record = (await _meetingDataProvider.GetMeetingRecordsAsync(
            command.MeetingRecordId, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (record?.UrlStatus != MeetingRecordUrlStatus.Pending)
            return new GeneralOverRecordingEvent();
        
        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(command.MeetingId, cancellationToken).ConfigureAwait(false);

        if (_currentUser.Id == null) return new GeneralOverRecordingEvent();
        
        var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id.Value, cancellationToken: cancellationToken).ConfigureAwait(false);

        var recordMeetingToken = _liveKitServerUtilService.GenerateTokenForRecordMeeting(user, meeting.MeetingNumber);

        var stopResponse = await _liveKitClient.StopEgressAsync(
            new StopEgressRequestDto { Token = recordMeetingToken, EgressId = record.EgressId }, cancellationToken).ConfigureAwait(false);

        Log.Information("background job stop meeting recording response: {@stopResponse}", stopResponse);
        
        var storageCommand = new DelayedMeetingRecordingStorageCommand 
        { 
            StartDate = _clock.Now, 
            Token = recordMeetingToken, 
            MeetingRecordId = command.MeetingRecordId,
            MeetingId = command.MeetingId, 
            EgressId = record.EgressId,
            ReTryLimit = command.ReTryLimit,
            IsRestartRecord = true
        };
        
        _sugarTalkBackgroundJobClient.Schedule<IMediator>(m => m.SendAsync(storageCommand, cancellationToken), TimeSpan.FromSeconds(10));

        return new GeneralOverRecordingEvent();
    }

    public async Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(GetMeetingRecordDetailsRequest request, CancellationToken cancellationToken)
    {
        return await _meetingDataProvider.GetMeetingRecordDetailsAsync(request.Id, request.Language, cancellationToken).ConfigureAwait(false);
    }

    public async Task<StorageMeetingRecordVideoResponse> StorageMeetingRecordVideoAsync(StorageMeetingRecordVideoCommand command, CancellationToken cancellationToken)
    {
        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(command.MeetingId, cancellationToken).ConfigureAwait(false);

        var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id.Value, cancellationToken: cancellationToken).ConfigureAwait(false);

        var recordMeetingToken = _liveKitServerUtilService.GenerateTokenForRecordMeeting(user, meeting.MeetingNumber);
        
        await _meetingDataProvider.UpdateMeetingRecordUrlStatusAsync(command.MeetingRecordId, MeetingRecordUrlStatus.InProgress, cancellationToken).ConfigureAwait(false);

        var stopResponse = await _liveKitClient.StopEgressAsync(
            new StopEgressRequestDto { Token = recordMeetingToken, EgressId = command.EgressId }, cancellationToken).ConfigureAwait(false);

        Log.Information("stop meeting recording response: {@stopResponse}", stopResponse);
        
        var speakDetails = await _meetingDataProvider.GetMeetingSpeakDetailsAsync(
            meetingNumber: meeting.MeetingNumber, recordId: command.MeetingRecordId, cancellationToken: cancellationToken).ConfigureAwait(false);

        foreach (var speakDetail in speakDetails.Where(speakDetail => speakDetail.SpeakEndTime is null or 0))
        {
            speakDetail.SpeakStatus = SpeakStatus.End;
            speakDetail.SpeakEndTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        
        await _meetingDataProvider.UpdateMeetingSpeakDetailsAsync(speakDetails, true, cancellationToken).ConfigureAwait(false);

        if (stopResponse == null) throw new Exception();

        var storageCommand = new DelayedMeetingRecordingStorageCommand 
        { 
            StartDate = _clock.Now, 
            Token = recordMeetingToken, 
            MeetingRecordId = command.MeetingRecordId,
            MeetingId = command.MeetingId, 
            EgressId = command.EgressId,
            ReTryLimit = command.ReTryLimit,
            IsRestartRecord = false
        };

        meeting.IsActiveRecord = false;

        await _meetingDataProvider.UpdateMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);
        
        _backgroundJobClient.Schedule<IMediator>(m => m.SendAsync(storageCommand, cancellationToken), TimeSpan.FromSeconds(10)); 
      
        return new StorageMeetingRecordVideoResponse();
    }

    public async Task<DelayedMeetingRecordingStorageEvent> ExecuteStorageMeetingRecordVideoDelayedJobAsync(
        DelayedMeetingRecordingStorageCommand command, CancellationToken cancellationToken)
    {
        Log.Information("Starting Execute Storage Meeting Record Video, staring time :{@StartTime}", command.StartDate );
       
        if (!command.IsRestartRecord)
        {
            var currentTime = _clock.Now;
        
            var timeElapsedSinceStart = (currentTime - command.StartDate).TotalMinutes;

            if (timeElapsedSinceStart > 5)
            {
                await _meetingDataProvider.UpdateMeetingRecordUrlStatusAsync(command.MeetingRecordId, MeetingRecordUrlStatus.Failed, cancellationToken).ConfigureAwait(false);
            
                _sugarTalkBackgroundJobClient.RemoveRecurringJobIfExists(nameof(ExecuteStorageMeetingRecordVideoDelayedJobAsync));
            }
        }

        return new DelayedMeetingRecordingStorageEvent
        {
            MeetingId = command.MeetingId,
            EgressId = command.EgressId,
            MeetingRecordId = command.MeetingRecordId,
            Token = command.Token,
            ReTryLimit = command.ReTryLimit,
            IsRestartRecord = command.IsRestartRecord
        };
    }

    public async Task  DelayStorageMeetingRecordVideoJobAsync(Guid meetingId, string egressId, Guid meetingRecordId, string token, int reTryLimit, bool isRestartRecording, CancellationToken cancellationToken)
    {
        var meetingRecord = await _meetingDataProvider.GetMeetingRecordByMeetingRecordIdAsync(meetingRecordId, cancellationToken).ConfigureAwait(false);
        if (meetingRecord == null) throw new MeetingRecordNotFoundException();

        var getEgressResponse = await _liveKitClient.GetEgressInfoListAsync(new GetEgressRequestDto { Token = token, EgressId = egressId }, cancellationToken).ConfigureAwait(false);
        
        Log.Information("get egress info list response: {@egressInfo}", getEgressResponse);
        
        var egressItem = getEgressResponse?.EgressItems.FirstOrDefault(x => x.EgressId == egressId && x.Status == "EGRESS_COMPLETE");

        if (egressItem == null)
            switch (reTryLimit > 0)
            {
                case true:
                    reTryLimit--;
                    _sugarTalkBackgroundJobClient.Enqueue<IMeetingService>(x =>
                        x.DelayStorageMeetingRecordVideoJobAsync(meetingId, egressId, meetingRecordId, token, reTryLimit, false, cancellationToken));
                    return;
                default:
                    if (isRestartRecording) break;
                    
                    meetingRecord.UrlStatus = MeetingRecordUrlStatus.Failed;
                    await _meetingDataProvider.UpdateMeetingRecordAsync(meetingRecord, cancellationToken).ConfigureAwait(false);
                    return;
            }

        if (isRestartRecording)
        {
            await HandleMeetingRecordRestartAsync(meetingId, meetingRecordId, egressItem, cancellationToken).ConfigureAwait(false);
            
            return;
        }
        
        egressItem.File.Location = await CombineMeetingRecordVideoAsync(meetingId, meetingRecordId, cancellationToken).ConfigureAwait(false);
        
        await UpdateMeetingRecordAsync(meetingRecord, egressItem, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> CombineMeetingRecordVideoAsync(Guid meetingId, Guid meetingRecordId, CancellationToken cancellationToken)
    {
        var urls = (await _meetingDataProvider.GetMeetingRecordVoiceRelayStationAsync(
                meetingId, meetingRecordId, cancellationToken).ConfigureAwait(false))
            .Where(x => !x.Url.IsNullOrEmpty())
            .Select(x => x.Url);
        
        var urlBytes = new List<byte[]>();
        
        foreach (var url in urls)
        {
            var urlContent = await _httpClientFactory.GetAsync<byte[]>(url, cancellationToken).ConfigureAwait(false);

            urlBytes.Add(urlContent);
        }
        
        try
        {
            var response = await _ffmpegService.CombineMp4VideosAsync(urlBytes, cancellationToken).ConfigureAwait(false);

            var fileName = $"SugarTalk/{Guid.NewGuid()}.mp4";
            
            _aliYunOssService.UploadFile(fileName, response);

            return _aliYunOssService.GetFileUrl(fileName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, @"Combine url upload failed, {urls}", JsonConvert.SerializeObject(urls)); 
            throw;
        }
    }

    private async Task HandleMeetingRecordRestartAsync(Guid meetingId, Guid meetingRecordId, EgressItemDto egressItem, CancellationToken cancellationToken)
    {
        var meetingRecordRestart = (await _meetingDataProvider.GetMeetingRecordVoiceRelayStationAsync(
            meetingId, meetingRecordId, cancellationToken).ConfigureAwait(false)).MaxBy(x => x.CreatedDate);

        await UpdateMeetingRestartRecordAsync(meetingRecordRestart, egressItem, cancellationToken).ConfigureAwait(false);
            
        await _meetingDataProvider.AddMeetingRecordVoiceRelayStationAsync(new MeetingRestartRecord
        {
            Id = new Guid(),
            MeetingId = meetingId,
            RecordId = meetingRecordId
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        var restartRecordCommand = new StartMeetingRecordingCommand
        {
            MeetingId = meetingId,
            MeetingRecordId = meetingRecordId,
            IsRestartRecord = true
        };
            
        _sugarTalkBackgroundJobClient.Enqueue<IMediator>(m => m.SendAsync(restartRecordCommand, cancellationToken).ConfigureAwait(false));
    }

    private async Task UpdateMeetingRestartRecordAsync(MeetingRestartRecord meetingRestartRecord, EgressItemDto egressItem, CancellationToken cancellationToken)
    {
        meetingRestartRecord.Url = egressItem.File.Location;
        
        Log.Information(@"Complete restart meeting record: {@recordRestart}", meetingRestartRecord);
        
        await _meetingDataProvider.UpdateMeetingRecordVoiceRelayStationAsync(meetingRestartRecord, cancellationToken).ConfigureAwait(false);
    }

    private async Task UpdateMeetingRecordAsync(MeetingRecord meetingRecord, EgressItemDto egressItem, CancellationToken cancellationToken)
    {
        meetingRecord.Url = egressItem.File.Location;
        meetingRecord.RecordType = MeetingRecordType.EndRecord;
        meetingRecord.UrlStatus = MeetingRecordUrlStatus.Completed;
    
        Log.Information("Complete storage meeting record url");
    
        await _meetingDataProvider.UpdateMeetingRecordAsync(meetingRecord, cancellationToken).ConfigureAwait(false);

        var meetingDetails = await _meetingDataProvider.GetMeetingDetailsByRecordIdAsync(meetingRecord.Id, cancellationToken).ConfigureAwait(false);
        
        if (!string.IsNullOrEmpty(meetingRecord.Url))
        {   
            await MarkSpeakTranscriptAsSpecifiedStatusAsync(meetingDetails, FileTranscriptionStatus.InProcess, cancellationToken).ConfigureAwait(false);

            await TranscriptionMeetingAsync(meetingDetails, meetingRecord, cancellationToken);
        }
    }

    public async Task<TranslatingMeetingSpeakResponse> TranslatingMeetingSpeakAsync(TranslatingMeetingSpeakCommand command, CancellationToken cancellationToken)
    {
        var meetingRecordDetails = await _meetingDataProvider.GetMeetingDetailsByRecordIdAsync(command.MeetingRecordId, cancellationToken).ConfigureAwait(false);
        
        var meetingTranslationRecordDetails = await _meetingDataProvider.GetMeetingDetailsTranslationRecordAsync(command.MeetingRecordId, command.Language, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (meetingTranslationRecordDetails is { Count: 0 })
        {
            var addTranslationRecords = meetingRecordDetails.Select(x => new MeetingSpeakDetailTranslationRecord
            {
                Language = command.Language,
                MeetingSpeakDetailId = x.Id,
                MeetingRecordId = x.MeetingRecordId,
                Status = MeetingSpeakTranslatingStatus.Progress
            }).ToList();
            
            await _meetingDataProvider.AddMeetingDetailsTranslationRecordAsync(addTranslationRecords, cancellationToken).ConfigureAwait(false);
            
            _backgroundJobClient.Enqueue(() => GenerateProcessSpeakTranslationAsync(addTranslationRecords, meetingRecordDetails, command.Language, cancellationToken));
        }
        
        return new TranslatingMeetingSpeakResponse
        {
            Data = (await _meetingDataProvider.GetMeetingRecordDetailsAsync(command.MeetingRecordId, command.Language, cancellationToken).ConfigureAwait(false)).Data
        };
    }

    public async Task GenerateProcessSpeakTranslationAsync(List<MeetingSpeakDetailTranslationRecord> meetingSpeeches, List<MeetingSpeakDetail> meetingRecordDetails, TranslationLanguage language, CancellationToken cancellationToken)
    {
        foreach (var speak in meetingRecordDetails)
        {
            try
            {
                var originalTranslationContent = (await _translationClient.TranslateTextAsync(
                        speak.OriginalContent, language.GetDescription(), cancellationToken: cancellationToken).ConfigureAwait(false)).TranslatedText;

                var smartTranslationContent = (await _translationClient.TranslateTextAsync(
                        speak.SmartContent, language.GetDescription(), cancellationToken: cancellationToken).ConfigureAwait(false)).TranslatedText;

                await _meetingDataProvider.UpdateMeetingDetailTranslationRecordAsync(meetingSpeeches.Where(x => x.MeetingSpeakDetailId == speak.Id).Select(x =>
                {
                    x.Status = MeetingSpeakTranslatingStatus.Completed;
                    x.SmartTranslationContent = smartTranslationContent;
                    x.OriginalTranslationContent = originalTranslationContent;

                    return x;
                }).FirstOrDefault(), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await _meetingDataProvider.UpdateMeetingDetailTranslationRecordAsync(meetingSpeeches.Where(x => x.MeetingSpeakDetailId == speak.Id).Select(x =>
                {
                    x.Status = MeetingSpeakTranslatingStatus.Exception;

                    return x;
                }).FirstOrDefault(), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}