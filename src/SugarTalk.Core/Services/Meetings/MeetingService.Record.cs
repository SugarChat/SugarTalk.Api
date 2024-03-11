using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Net;
using Serilog;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Dto.Translation;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Messages.Events.Meeting.Summary;
using SugarTalk.Messages.Extensions;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<GetCurrentUserMeetingRecordResponse> GetCurrentUserMeetingRecordsAsync(GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken);
    
    Task<MeetingRecordDeletedEvent> DeleteMeetingRecordAsync(DeleteMeetingRecordCommand command, CancellationToken cancellationToken);

    Task<MeetingRecordingStartedEvent> StartMeetingRecordingAsync(StartMeetingRecordingCommand command, CancellationToken cancellationToken);
    
    Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(GetMeetingRecordDetailsRequest request, CancellationToken cancellationToken);
    
    Task<StorageMeetingRecordVideoResponse> StorageMeetingRecordVideoAsync(StorageMeetingRecordVideoCommand command, CancellationToken cancellationToken);
    
    Task<DelayedMeetingRecordingStorageEvent> ExecuteStorageMeetingRecordVideoDelayedJobAsync(DelayedMeetingRecordingStorageCommand command, CancellationToken cancellationToken);

    Task DelayStorageMeetingRecordVideoJobAsync(string egressId, Guid meetingRecordId, string token, int reTryLimit, CancellationToken cancellationToken);
    
    Task<GetMeetingSpeakTranslationResponse> GetMeetingSpeakTranslationAsync(GetMeetingSpeakTranslationRequest request, CancellationToken cancellationToken);
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
        var meetingRecordId = Guid.NewGuid();
        
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
                FilePath = $"SugarTalk/{meetingRecordId}.mp4",
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

        await _meetingDataProvider.PersistMeetingRecordAsync(meeting.Id, meetingRecordId, postResponse.EgressId, cancellationToken).ConfigureAwait(false);

        return new MeetingRecordingStartedEvent
        {
            MeetingRecordId = meetingRecordId,
            EgressId = postResponse.EgressId
        };
    }
    
    public async Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(GetMeetingRecordDetailsRequest request, CancellationToken cancellationToken)
    {
        return await _meetingDataProvider.GetMeetingRecordDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
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
        
        if (stopResponse == null) throw new Exception();

        var storageCommand = new DelayedMeetingRecordingStorageCommand 
        { 
            StartDate = _clock.Now, 
            Token = recordMeetingToken, 
            MeetingRecordId = command.MeetingRecordId,
            MeetingId = command.MeetingId, 
            EgressId = command.EgressId,
            ReTryLimit = command.ReTryLimit
        }; 
        
        _backgroundJobClient.Schedule<IMediator>(m=>m.SendAsync(storageCommand, cancellationToken), TimeSpan.FromSeconds(10)); 
      
        return new StorageMeetingRecordVideoResponse();
    }

    public async Task<DelayedMeetingRecordingStorageEvent> ExecuteStorageMeetingRecordVideoDelayedJobAsync(
        DelayedMeetingRecordingStorageCommand command, CancellationToken cancellationToken)
    {
        Log.Information("Starting Execute Storage Meeting Record Video, staring time :{@StartTime}", command.StartDate );
        
        var currentTime = _clock.Now;
        
        var timeElapsedSinceStart = (currentTime - command.StartDate).TotalMinutes;

        if (timeElapsedSinceStart > 5)
        {
            await _meetingDataProvider.UpdateMeetingRecordUrlStatusAsync(command.MeetingRecordId, MeetingRecordUrlStatus.Failed, cancellationToken).ConfigureAwait(false);
            
            _sugarTalkBackgroundJobClient.RemoveRecurringJobIfExists(nameof(ExecuteStorageMeetingRecordVideoDelayedJobAsync));
        }

        return new DelayedMeetingRecordingStorageEvent
        {
            MeetingId = command.MeetingId,
            EgressId = command.EgressId,
            MeetingRecordId = command.MeetingRecordId,
            Token = command.Token,
            ReTryLimit = command.ReTryLimit
        };
    }

    public async Task DelayStorageMeetingRecordVideoJobAsync(string egressId, Guid meetingRecordId, string token, int reTryLimit, CancellationToken cancellationToken)
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
                        x.DelayStorageMeetingRecordVideoJobAsync(egressId, meetingRecordId, token, reTryLimit, cancellationToken));
                    return;
                default:
                    meetingRecord.UrlStatus = MeetingRecordUrlStatus.Failed;
                    await _meetingDataProvider.UpdateMeetingRecordAsync(meetingRecord, cancellationToken).ConfigureAwait(false);
                    return;
            }

        meetingRecord.Url = egressItem.File.Location;
        meetingRecord.RecordType = MeetingRecordType.EndRecord;
        meetingRecord.UrlStatus = MeetingRecordUrlStatus.Completed;
    
        Log.Information("Complete storage meeting record url");
    
        await _meetingDataProvider.UpdateMeetingRecordAsync(meetingRecord, cancellationToken).ConfigureAwait(false);

        var meetingDetails = await _meetingDataProvider.GetMeetingDetailsByRecordIdAsync(meetingRecordId, cancellationToken).ConfigureAwait(false);

        foreach (var meetingDetail in meetingDetails)
        {
            await TranscriptionMeetingAsync(meetingDetail, meetingRecord, cancellationToken);
        }
    }

    public async Task<GetMeetingSpeakTranslationResponse> GetMeetingSpeakTranslationAsync(
        GetMeetingSpeakTranslationRequest request, CancellationToken cancellationToken)
    {
        var meetingRecordDetails = await _meetingDataProvider.GetMeetingDetailsByRecordIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        
        var meetingTranslationRecordDetails = await _meetingDataProvider.GetMeetingDetailsTranslationRecordAsync(request.Id, request.Language, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        if (meetingTranslationRecordDetails is { Count: > 0 })
            return new GetMeetingSpeakTranslationResponse
            {
               Data = new GetMeetingSpeakTranslationDto
               {
                   MeetingSpeakDetail = meetingRecordDetails.Select(x => _mapper.Map<MeetingSpeakDetailDto>(x)).ToList(),
                   MeetingSpeakTranslationDetail = meetingTranslationRecordDetails.Select(x => _mapper.Map<MeetingSpeakTranslationDetailDto>(x)).ToList()
               }
            };
        
        var addTranslationRecords = new List<MeetingSpeakDetailTranslationRecord>();

        foreach (var speak in meetingRecordDetails)
        {
            addTranslationRecords.Add(new MeetingSpeakDetailTranslationRecord
            {
                Language = request.Language,
                MeetingRecordId = request.Id,
                MeetingSpeakDetailId = speak.Id
            });
            
            _backgroundJobClient.Enqueue(() => GenerateProcessSpeakTranslationAsync(speak, request.Language, cancellationToken));
        }
        
        await _meetingDataProvider.AddMeetingDetailsTranslationRecordAsync(addTranslationRecords, cancellationToken).ConfigureAwait(false);
        
        var againMeetingTranslationRecordDetails = await _meetingDataProvider.GetMeetingDetailsTranslationRecordAsync(request.Id, request.Language, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        return new GetMeetingSpeakTranslationResponse
        {
            Data = new GetMeetingSpeakTranslationDto
            {
                MeetingSpeakDetail = meetingRecordDetails.Select(x => _mapper.Map<MeetingSpeakDetailDto>(x)).ToList(),
                MeetingSpeakTranslationDetail = againMeetingTranslationRecordDetails.Select(x => _mapper.Map<MeetingSpeakTranslationDetailDto>(x)).ToList()
            }
        };
    }
    
    private async Task GenerateProcessSpeakTranslationAsync(MeetingSpeakDetail meetingSpeech, TranslationLanguage language, CancellationToken cancellationToken)
    {
        var originalTranslationContent =  (await _translationClient.TranslateTextAsync(meetingSpeech.OriginalContent, language.GetDescription(), cancellationToken: cancellationToken).ConfigureAwait(false)).TranslatedText;
            
        var smartTranslationContent = (await _translationClient.TranslateTextAsync(meetingSpeech.SmartContent, language.GetDescription(), cancellationToken: cancellationToken).ConfigureAwait(false)).TranslatedText;

        var meetingSpeakDetailTranslation = (await _meetingDataProvider.GetMeetingDetailsTranslationRecordAsync(
        meetingSpeech.MeetingRecordId, language, meetingSpeech.Id, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (meetingSpeakDetailTranslation != null)
        {
            meetingSpeakDetailTranslation.Status = MeetingBackLoadingStatus.Completed;
            meetingSpeakDetailTranslation.OriginalTranslationContent = originalTranslationContent;
            meetingSpeakDetailTranslation.SmartTranslationContent = smartTranslationContent;
        }

        await _meetingDataProvider.UpdateMeetingDetailsTranslationRecordAsync(meetingSpeakDetailTranslation, cancellationToken).ConfigureAwait(false);
    }
}