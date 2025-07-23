using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using Serilog;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.TencentCloud;
using SugarTalk.Messages.Commands.Tencent;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Trtc.V20190722;
using TencentCloud.Trtc.V20190722.Models;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ITencentClient : IScopedDependency
{
    Task<StartCloudRecordingResponse> CreateCloudRecordingAsync(CreateCloudRecordingRequest request, CancellationToken cancellationToken);

    Task<StopCloudRecordingResponse> StopCloudRecordingAsync(DeleteCloudRecordingRequest request, CancellationToken cancellationToken);

    Task<UpdateCloudRecordingResponse> ModifyCloudRecordingAsync(ModifyCloudRecordingRequest request, CancellationToken cancellationToken);
}

public class TencentClient : ITencentClient
{
    private readonly IMapper _mapper;
    private readonly TencentCloudSetting _tencentCloudSetting;

    public TencentClient(IMapper mapper, TencentCloudSetting tencentCloudSetting)
    {
        _mapper = mapper;
        _tencentCloudSetting = tencentCloudSetting;
    }
    
    public TrtcClient CreateClient()
    {
        var cred = new Credential
        {
            SecretId = _tencentCloudSetting.SecretId,
            SecretKey = _tencentCloudSetting.SecretKey
        };

        return new TrtcClient(cred, _tencentCloudSetting.Region);
    }

    public async Task<StartCloudRecordingResponse> CreateCloudRecordingAsync(CreateCloudRecordingRequest request, CancellationToken cancellationToken)
    {
        var client = CreateClient();

        request.StorageParams = new StorageParams
        {
            CloudStorage = new CloudStorage
            {
                Vendor = 0,
                Bucket = _tencentCloudSetting.CosBucket,
                Region = _tencentCloudSetting.Region,
                AccessKey = _tencentCloudSetting.CosAccessKey,
                SecretKey = _tencentCloudSetting.CosSecretKey,
            }
        };
        
        Log.Information("CreateCloudRecordingAsync request: {request}", request);
        
        var response = await client.CreateCloudRecording(request).ConfigureAwait(false);
        
        Log.Information("CreateCloudRecordingAsync response: {response}", response);
        
        Log.Information("TaskId: {taskId}, RequestId: {requestId}", response.TaskId, response.RequestId);

        return new StartCloudRecordingResponse()
        {
            Data = _mapper.Map<CreateCloudRecordingResponseResult>(response)
        };
    }
    
    public async Task<StopCloudRecordingResponse> StopCloudRecordingAsync(DeleteCloudRecordingRequest request, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        
        Log.Information("StopCloudRecordingAsync request: {request}", request);
        
        var response = await client.DeleteCloudRecording(request).ConfigureAwait(false);
        
        Log.Information("StopCloudRecordingAsync response: {response}", response);
       
        return new StopCloudRecordingResponse()
        {
            Data = _mapper.Map<StopCloudRecordingResponseResult>(response)
        };
    }
    
    public async Task<UpdateCloudRecordingResponse> ModifyCloudRecordingAsync(ModifyCloudRecordingRequest request, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        
        Log.Information("ModifyCloudRecordingAsync request: {request}", request);
        
        var response = await client.ModifyCloudRecording(request).ConfigureAwait(false);
        
        Log.Information("ModifyCloudRecordingAsync response: {response}", response);

        return new UpdateCloudRecordingResponse()
        {
            Data = _mapper.Map<UpdateCloudRecordingResponseResult>(response)
        };
    }
}