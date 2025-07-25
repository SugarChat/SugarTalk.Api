using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using Serilog;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.TencentCloud;
using SugarTalk.Messages.Commands.Tencent;
using SugarTalk.Messages.Dto.Tencent;
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

        request.UserId = Guid.NewGuid().ToString();
        request.UserSig = GetUserSig(request.UserId);
        request.StorageParams = new StorageParams
        {
            CloudStorage = new CloudStorage
            {
                Vendor = 0,
                Bucket = _tencentCloudSetting.CosBucket,
                Region = _tencentCloudSetting.Region,
                AccessKey = _tencentCloudSetting.SecretId,
                SecretKey = _tencentCloudSetting.SecretKey,
                FileNamePrefix = new string[]{ _tencentCloudSetting.CosFileNamePrefix }
            }
        };
        
        Log.Information("CreateCloudRecordingAsync request: {@request}", request);
        
        var response = await client.CreateCloudRecording(request).ConfigureAwait(false);
        
        Log.Information("CreateCloudRecordingAsync response: {@response}", response);

        return new StartCloudRecordingResponse()
        {
            Data = _mapper.Map<CreateCloudRecordingResponseResult>(response)
        };
    }
    
    public async Task<StopCloudRecordingResponse> StopCloudRecordingAsync(DeleteCloudRecordingRequest request, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        
        Log.Information("StopCloudRecordingAsync request: {@request}", request);
        
        var response = await client.DeleteCloudRecording(request).ConfigureAwait(false);
        
        Log.Information("StopCloudRecordingAsync response: {@response}", response);
        
        return new StopCloudRecordingResponse()
        {
            Data = _mapper.Map<StopCloudRecordingResponseResult>(response)
        };
    }
    
    public async Task<UpdateCloudRecordingResponse> ModifyCloudRecordingAsync(ModifyCloudRecordingRequest request, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        
        Log.Information("ModifyCloudRecordingAsync request: {@request}", request);
        
        var response = await client.ModifyCloudRecording(request).ConfigureAwait(false);
        
        Log.Information("ModifyCloudRecordingAsync response: {@response}", response);

        return new UpdateCloudRecordingResponse()
        {
            Data = _mapper.Map<UpdateCloudRecordingResponseResult>(response)
        };
    }
    
    public string GetUserSig(string userId)
    {
        var api = new TencentTlsSigApIv2(int.Parse(_tencentCloudSetting.AppId), _tencentCloudSetting.SDKSecretKey);
        return api.GenSig(Utf16To8(userId));
    }
    
    public static string Utf16To8(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var utf16Bytes = Encoding.Unicode.GetBytes(input);
        var utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);
        return Encoding.UTF8.GetString(utf8Bytes);
    }
}