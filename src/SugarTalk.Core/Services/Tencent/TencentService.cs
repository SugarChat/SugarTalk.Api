using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Serilog;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Settings.TencentCloud;
using SugarTalk.Messages.Commands.Tencent;
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
    private readonly TencentClient _tencentClient;
    private readonly TencentCloudSetting _tencentCloudSetting;

    public TencentService(IMapper mapper, TencentClient tencentClient, TencentCloudSetting tencentCloudSetting)
    {
        _mapper = mapper;
        _tencentClient = tencentClient;
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
        return await _tencentClient.CreateCloudRecordingAsync(_mapper.Map<CreateCloudRecordingRequest>(command), cancellationToken).ConfigureAwait(false);
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
        Log.Information("CloudRecordingCallBackAsync  command: {@command}, eventType: {@eventType}", command, command.EventType.GetDescription());
    }
}