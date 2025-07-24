using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Serilog;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Settings.TencentCloud;
using SugarTalk.Messages.Commands.Tencent;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Tencent;
using TencentCloud.Trtc.V20190722.Models;

namespace SugarTalk.Core.Services.Tencent;

public interface ITencentService : IScopedDependency
{
    GetTencentCloudKeyResponse GetTencentCloudKey(GetTencentCloudKeyRequest request);
    
    Task<StartCloudRecordingResponse> CreateCloudRecordingAsync(CreateCloudRecordingCommand command, CancellationToken cancellationToken);
    
    Task<StopCloudRecordingResponse> StopCloudRecordingAsync(StopCloudRecordingCommand command, CancellationToken cancellationToken);
    
    Task<UpdateCloudRecordingResponse> UpdateCloudRecordingAsync(UpdateCloudRecordingCommand command, CancellationToken cancellationToken);
}

public class TencentService : ITencentService
{
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly TencentClient _tencentClient;
    private readonly IMeetingDataProvider _meetingDataProvider;
    private readonly TencentCloudSetting _tencentCloudSetting;

    public TencentService(IMapper mapper, ICurrentUser currentUser, TencentClient tencentClient, IMeetingDataProvider meetingDataProvider, TencentCloudSetting tencentCloudSetting)
    {
        _mapper = mapper;
        _currentUser = currentUser;
        _tencentClient = tencentClient;
        _meetingDataProvider = meetingDataProvider;
        _tencentCloudSetting = tencentCloudSetting;
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
}