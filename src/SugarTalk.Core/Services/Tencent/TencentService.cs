using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using Serilog;
using SugarTalk.Core.Domain.Meeting;
using Serilog;
using SugarTalk.Core.Domain.SpeechMatics;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Aws;
using SugarTalk.Core.Services.Caching;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Core.Services.Ffmpeg;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Services.Smarties;
using SugarTalk.Core.Settings.Smarties;
using SugarTalk.Core.Settings.TencentCloud;
using SugarTalk.Messages.Commands.Tencent;
using SugarTalk.Messages.Dto.FClub;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Smarties;
using SugarTalk.Messages.Dto.Tencent;
using SugarTalk.Messages.Enums.Caching;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Enums.Meeting.Speak;
using SugarTalk.Messages.Enums.Smarties;
using SugarTalk.Messages.Enums.SpeechMatics;
using SugarTalk.Messages.Enums.Tencent;
using SugarTalk.Messages.Extensions;
using SugarTalk.Messages.Requests.Tencent;
using TencentCloud.Trtc.V20190722.Models;

namespace SugarTalk.Core.Services.Tencent;

public interface ITencentService : IScopedDependency
{
    GetTencentCloudKeyResponse GetTencentCloudKey(GetTencentCloudKeyRequest request);
    
    Task<StartCloudRecordingResponse> CreateCloudRecordingAsync(CreateCloudRecordingCommand command, CancellationToken cancellationToken);
    
    Task<StopCloudRecordingResponse> StopCloudRecordingAsync(StopCloudRecordingCommand command, CancellationToken cancellationToken);
    
    Task<UpdateCloudRecordingResponse> UpdateCloudRecordingAsync(UpdateCloudRecordingCommand command, CancellationToken cancellationToken);

    Task CloudRecordingCallBackAsync(CloudRecordingCallBackCommand command, CancellationToken cancellationToken);
}

public class TencentService : ITencentService
{
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly IFClubClient _fclubClient;
    private readonly ICacheManager _cacheManager;
    private readonly TencentClient _tencentClient;
    private readonly IFfmpegService _ffmpegService;
    private readonly ISmartiesClient _smartiesClient;
    private readonly SmartiesSettings _smartiesSettings;
    private readonly ISmartiesDataProvider _smartiesDataProvider;
    private readonly IMeetingDataProvider _meetingDataProvider;
    private readonly TencentCloudSetting _tencentCloudSetting;
    private readonly IAccountDataProvider _accountDataProvider;
    
    public TencentService(
        IMapper mapper,
        ICurrentUser currentUser,
        IFClubClient fclubClient,
        ICacheManager cacheManager,
        TencentClient tencentClient,
        IFfmpegService ffmpegService,
        ISmartiesClient smartiesClient,
        SmartiesSettings smartiesSettings,
        ISmartiesDataProvider smartiesDataProvider,
        IMeetingDataProvider meetingDataProvider,
        TencentCloudSetting tencentCloudSetting,
        IAccountDataProvider accountDataProvider)
    {
        _mapper = mapper;
        _currentUser = currentUser;
        _fclubClient = fclubClient;
        _cacheManager = cacheManager;
        _tencentClient = tencentClient;
        _ffmpegService = ffmpegService;
        _smartiesClient = smartiesClient;
        _smartiesSettings = smartiesSettings;
        _smartiesDataProvider = smartiesDataProvider;
        _meetingDataProvider = meetingDataProvider;
        _tencentCloudSetting = tencentCloudSetting;
        _accountDataProvider = accountDataProvider;
    }

    public GetTencentCloudKeyResponse GetTencentCloudKey(GetTencentCloudKeyRequest request)
    {
        return new GetTencentCloudKeyResponse
        {
            Data = new GetTencentCloudKeyResponseData
            {
                AppId = _tencentCloudSetting.AppId,
                SDKSecretKey = _tencentCloudSetting.SDKSecretKey
            }
        };
    }

    public async Task<StartCloudRecordingResponse> CreateCloudRecordingAsync(CreateCloudRecordingCommand command, CancellationToken cancellationToken)
    {
        var meetingRecordId = Guid.NewGuid();
        
        var meeting = await _meetingDataProvider
            .GetMeetingByIdAsync(meetingNumber: command.RoomId, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (meeting is null) throw new MeetingNotFoundException();
        
        var recordingTask = await _tencentClient.CreateCloudRecordingAsync(_mapper.Map<CreateCloudRecordingRequest>(command), cancellationToken).ConfigureAwait(false);
        
        Log.Information("Created recording task result: {@recordingTask}", recordingTask);
        
        await AddMeetingRecordAsync(meeting, meetingRecordId, recordingTask.Data.TaskId, _currentUser.Id.Value, cancellationToken).ConfigureAwait(false);

        recordingTask.Data.MeetingRecordId = meetingRecordId;
        
        return recordingTask;
    }
    
    private async Task AddMeetingRecordAsync(Meeting meeting, Guid meetingRecordId, string egressId, int id, CancellationToken cancellationToken)
    {
        Log.Information("meeting: {@meeting}; meetingRecordId: {meetingRecordId}; egressId: {egressId}; id: {id}", meeting, meetingRecordId, egressId, id);
        
        MeetingUserSession userSession = null;
        
        if (meeting.AppointmentType == MeetingAppointmentType.Appointment)
            userSession = (await _meetingDataProvider.GetMeetingUserSessionsAsync(
                meetingId: meeting.Id, userIds: new List<int> { id }, onlineType: MeetingUserSessionOnlineType.Online,  cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        Log.Information("Add meeting record user session: {@userSession}", userSession);

        await _meetingDataProvider.PersistMeetingRecordAsync(meeting.Id, meetingRecordId, egressId, userSession?.MeetingSubId, cancellationToken).ConfigureAwait(false);
        
        meeting.IsActiveRecord = true;

        await _meetingDataProvider.UpdateMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);
    }

    public async Task<StopCloudRecordingResponse> StopCloudRecordingAsync(StopCloudRecordingCommand command, CancellationToken cancellationToken)
    {
        return await _tencentClient.StopCloudRecordingAsync(_mapper.Map<DeleteCloudRecordingRequest>(command), cancellationToken).ConfigureAwait(false);
    }

    public async Task<UpdateCloudRecordingResponse> UpdateCloudRecordingAsync(UpdateCloudRecordingCommand command, CancellationToken cancellationToken)
    {
        return await _tencentClient.ModifyCloudRecordingAsync(_mapper.Map<ModifyCloudRecordingRequest>(command), cancellationToken).ConfigureAwait(false);
    }

    public async Task CloudRecordingCallBackAsync(CloudRecordingCallBackCommand command, CancellationToken cancellationToken)
    {
        switch (command.EventType)
        {
            case CloudRecordingEventType.CloudRecordingMp4Stop:
                
                Log.Information("CloudRecordingCallBackAsync  command: {@command}, eventType: {@eventType}", command, command.EventType.GetDescription());
                
                var fileMessages = command.EventInfo.Payload.ToObject<CloudRecordingMp4StopPayloadDto>();
                
                await HandleRecordingCompletedAsync(command, fileMessages, cancellationToken).ConfigureAwait(false);
                
                break;
            case CloudRecordingEventType.CloudRecordingRecorderStop:

                await MarkSpeakTranscriptAsSpecifiedStatusAsync(command, cancellationToken).ConfigureAwait(false);
                
                break;
            default:
                Log.Information("CloudRecordingCallBackAsync  command: {@command}, eventType: {@eventType}", command, command.EventType.GetDescription());
                break;
        }
     
    }

    private async Task MarkSpeakTranscriptAsSpecifiedStatusAsync(CloudRecordingCallBackCommand command, CancellationToken cancellationToken)
    {
        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(meetingNumber: command.EventInfo.RoomId, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        var record = (await _meetingDataProvider.GetMeetingRecordsAsync(
            meeting.Id, cancellationToken: cancellationToken).ConfigureAwait(false)).MaxBy(x => x.CreatedDate);
        
        await _meetingDataProvider.UpdateMeetingRecordUrlStatusAsync(record.Id, MeetingRecordUrlStatus.InProgress, cancellationToken).ConfigureAwait(false);
        
        try
        {
            var participants = await _meetingDataProvider.GetUserSessionsByMeetingIdAsync(meeting.Id, record.MeetingSubId, true, true, true, cancellationToken).ConfigureAwait(false);
        
            var filterGuest = participants.Where(p => p.GuestName == null).ToList();
            Log.Information("filter guest response: {@filterGuest}", filterGuest);
        
            foreach (var participant in filterGuest)
            {
                await AddRecordForAccountAsync(participant.UserName, cancellationToken).ConfigureAwait(false);
                Log.Information("An exception occurred while processing participants: {@participant}", participant);
            }
            
            meeting.IsActiveRecord = false;

            await _meetingDataProvider.UpdateMeetingAsync(meeting, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            record.UrlStatus = MeetingRecordUrlStatus.Failed;
             
            await _meetingDataProvider.UpdateMeetingRecordAsync(record, cancellationToken).ConfigureAwait(false);
        }
    }
    
    private async Task AddRecordForAccountAsync(string currentUserName, CancellationToken cancellationToken)
    {
        var recordKey = currentUserName;
        
        var recordCount = await _cacheManager.GetOrAddAsync(recordKey, () => Task.FromResult(new RecordCountDto(recordKey)), CachingType.RedisCache, cancellationToken: cancellationToken).ConfigureAwait(false);

        recordCount.Count += 1;

        await _cacheManager.SetAsync(recordKey, recordCount, CachingType.RedisCache, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleRecordingCompletedAsync(CloudRecordingCallBackCommand command, CloudRecordingMp4StopPayloadDto payload, CancellationToken cancellationToken)
    {
        var record = (await _meetingDataProvider.GetMeetingRecordsAsync(egressId: command.EventInfo.TaskId, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        
        var url = "";
        var endTimeStamp = payload.FileMessage.Max(x => x.EndTimeStamp);
        
        var speakDetails = await _meetingDataProvider.GetMeetingSpeakDetailsAsync(
            meetingNumber: command.EventInfo.RoomId, recordId: record.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        foreach (var speakDetail in speakDetails.Where(speakDetail => speakDetail.SpeakEndTime is null or 0))
        {
            speakDetail.SpeakStatus = SpeakStatus.End;
            speakDetail.SpeakEndTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
            
        await _meetingDataProvider.UpdateMeetingSpeakDetailsAsync(speakDetails, true, cancellationToken).ConfigureAwait(false);
        
        if (payload.FileMessage.Count > 1)
        {
            var videoUrls = payload.FileMessage.Select(x => $"{_tencentCloudSetting.CosBaseUrl}/{_tencentCloudSetting.CosFileNamePrefix}/{command.EventInfo.TaskId}/{x.FileName}").ToList();
            
            url = await CombineMeetingRecordVideoAsync(record.Id, videoUrls, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(url)) return;
        }
        else url = $"{_tencentCloudSetting.CosBaseUrl}/{_tencentCloudSetting.CosFileNamePrefix}/{command.EventInfo.TaskId}/{payload.FileMessage.FirstOrDefault().FileName}";
        
        if (!string.IsNullOrEmpty(url))
        {
            record.Url = url;
            record.UrlStatus = MeetingRecordUrlStatus.Completed;
            record.EndedAt = DateTimeOffset.FromUnixTimeMilliseconds(endTimeStamp);
        }
        
        Log.Information($"Add url for record: meetingRecord: {record}", record);
        
        await _meetingDataProvider.UpdateMeetingRecordAsync(record, cancellationToken).ConfigureAwait(false);
        
        if (!string.IsNullOrEmpty(record.Url))
        {
            var meetingDetails = await _meetingDataProvider
                .GetMeetingDetailsByRecordIdAsync(record.Id, cancellationToken).ConfigureAwait(false);
             
            if (!meetingDetails.Any()) return;
            
            await MarkSpeakTranscriptAsSpecifiedStatusAsync(meetingDetails, FileTranscriptionStatus.InProcess, cancellationToken).ConfigureAwait(false);
            
            var localhostUrl = await DownloadWithRetryAsync(url, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            await CreateSpeechMaticsJobAsync(record, meetingDetails, localhostUrl, cancellationToken).ConfigureAwait(false);
        }
    }
    
    private async Task<string> CombineMeetingRecordVideoAsync(Guid meetingRecordId, List<string> videoUrls, CancellationToken cancellationToken)
    {
        Log.Information("Combine urls: {@urls}", videoUrls);
        
        try{
            var response = await _fclubClient.CombineMp4VideosTaskAsync(new CombineMp4VideosTaskDto
            {
                urls = videoUrls,
                UploadId = meetingRecordId,
                filePath = $"SugarTalk/{meetingRecordId}.mp4"
            }, cancellationToken).ConfigureAwait(false);
            
            return response.Data.ToString();
        }
        catch (Exception ex)
        {
            Log.Error(ex, @"Combine url upload failed, {urls}", JsonConvert.SerializeObject(videoUrls)); 
            throw;
        }
    }
    
    private async Task MarkSpeakTranscriptAsSpecifiedStatusAsync(
        List<MeetingSpeakDetail> details, FileTranscriptionStatus status, CancellationToken cancellationToken)
    {
        details.ForEach(x => x.FileTranscriptionStatus = status);
        
        await _meetingDataProvider.UpdateMeetingSpeakDetailsAsync(details, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
    
    private async Task CreateSpeechMaticsJobAsync(MeetingRecord meetingRecord, List<MeetingSpeakDetail> meetingDetails, string localhostUrl, CancellationToken cancellationToken)
    {
        Log.Information("Create speech matics job meeting record: {@meetingRecord} MeetingDetails:{@meetingDetails}", meetingRecord, meetingDetails);
         
        var audio = await _ffmpegService.VideoToAudioConverterAsync(localhostUrl, cancellationToken).ConfigureAwait(false);
         
        var speechMaticsJob = await _smartiesClient.CreateSpeechMaticsJobAsync(new CreateSpeechMaticsJobCommandDto
        {
            Language = "zh",
            RecordContent = audio,
            Url = _smartiesSettings.SpeechMaticsUrl,
            Key = _smartiesSettings.SpeechMaticsApiKey,
            SourceSystem = TranscriptionJobSystem.SmartTalk,
            RecordName = meetingRecord.Url
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("Create SpeechMatics job: {@speechMaticsJob}", speechMaticsJob.Result);

        var meetingSpeechMaticsRecord = new SpeechMaticsRecord
        {
            Status = SpeechMaticsStatus.Sent,
            MeetingRecordId = meetingRecord.Id,
            TranscriptionJobId = speechMaticsJob.Result,
            MeetingNumber = meetingDetails.FirstOrDefault()?.MeetingNumber
        };
         
        await _smartiesDataProvider.CreateSpeechMaticsRecordAsync(meetingSpeechMaticsRecord, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<string> DownloadWithRetryAsync(string url, int maxRetries = 5, CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < maxRetries; i++)
        {
            try
            {
                var temporaryFile = $"{Guid.NewGuid()}.mp4";
                
                Log.Information("Uploading url: {url}, temporaryFile: {temporaryFile}", url, temporaryFile);

                using var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                
                response.EnsureSuccessStatusCode();

                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

                await using var fileStream = new FileStream(temporaryFile, FileMode.Create, FileAccess.Write);
               
                await contentStream.CopyToAsync(fileStream, 8192, cancellationToken).ConfigureAwait(false);

                return temporaryFile;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Log.Warning($"Timeout occurred while downloading {url}, retrying... ({i + 1}/{maxRetries})");
                
                if (i != maxRetries - 1) continue;
                
                Log.Error($"Failed to download {url} after {maxRetries} retries.");
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error occurred while downloading {url}");
                throw;
            }
        }

        return null!;
    }
    
    private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
}