using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Commands.Meetings.Summary;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Events.Meeting.Summary;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<GetCurrentUserMeetingRecordResponse> GetCurrentUserMeetingRecordsAsync(GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken);
    
    Task DeleteMeetingRecordAsync(DeleteMeetingRecordCommand command, CancellationToken cancellationToken);

    Task<MeetingRecordingStartedEvent> StartMeetingRecordingAsync(StartMeetingRecordingCommand command, CancellationToken cancellationToken);
    
    Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(GetMeetingRecordDetailsRequest request, CancellationToken cancellationToken);
    
    Task<StorageMeetingRecordVideoResponse> StorageMeetingRecordVideoAsync(StorageMeetingRecordVideoCommand command, CancellationToken cancellationToken);
    
    Task<DelayedMeetingRecordingStorageEvent> ExecuteStorageMeetingRecordVideoDelayedJobAsync(DelayedMeetingRecordingStorageCommand command, CancellationToken cancellationToken);

    Task DelayStorageMeetingRecordVideoJobAsync(string egressId, Guid meetingRecordId, string token, CancellationToken cancellationToken);
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
        };
        
        _sugarTalkBackgroundJobClient.AddOrUpdateRecurringJob<MeetingService>(nameof(ExecuteStorageMeetingRecordVideoDelayedJobAsync),
            meetingService => meetingService.ExecuteStorageMeetingRecordVideoDelayedJobAsync(storageCommand, cancellationToken), "*/5 * * * * ?");

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
            Token = command.Token
        };
    }

    public async Task DelayStorageMeetingRecordVideoJobAsync(string egressId, Guid meetingRecordId, string token, CancellationToken cancellationToken)
    {
        var meetingRecord = await _meetingDataProvider.GetMeetingRecordByMeetingRecordIdAsync(meetingRecordId, cancellationToken).ConfigureAwait(false);
        if (meetingRecord == null) throw new MeetingRecordNotFoundException();

        var getEgressResponse = await _liveKitClient.GetEgressInfoListAsync(new GetEgressRequestDto { Token = token, EgressId = egressId }, cancellationToken).ConfigureAwait(false);
        
        Log.Information("get egress info list response: {@egressInfo}", getEgressResponse);
        
        var egressItemDto = getEgressResponse?.EgressItems.FirstOrDefault(x => x.EgressId == egressId && x.Status == "EGRESS_COMPLETE");

        if (egressItemDto == null) throw new Exception();

        meetingRecord.Url = egressItemDto.File.Location;
        meetingRecord.RecordType = MeetingRecordType.EndRecord;
        meetingRecord.UrlStatus = MeetingRecordUrlStatus.Completed;
        
        await _meetingDataProvider.UpdateMeetingRecordAsync(meetingRecord, cancellationToken).ConfigureAwait(false);
    }
}