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
using SugarTalk.Messages.Enums.Tencent;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Trtc.V20190722;
using TencentCloud.Trtc.V20190722.Models;
using MixTranscodeParams = TencentCloud.Trtc.V20190722.Models.MixTranscodeParams;
using RecordParams = TencentCloud.Trtc.V20190722.Models.RecordParams;
using VideoParams = TencentCloud.Trtc.V20190722.Models.VideoParams;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ITencentClient : IScopedDependency
{
    Task<StartCloudRecordingResponse> CreateCloudRecordingAsync(CreateCloudRecordingRequest request, CancellationToken cancellationToken);

    Task<StopCloudRecordingResponse> StopCloudRecordingAsync(DeleteCloudRecordingRequest request, CancellationToken cancellationToken);

    Task<UpdateCloudRecordingResponse> ModifyCloudRecordingAsync(ModifyCloudRecordingRequest request, CancellationToken cancellationToken);

    Task DisbandRoomAsync(DismissRoomRequest request, CancellationToken cancellationToken);
    
    Task DismissRoomByStrRoomIdAsync(DismissRoomByStrRoomIdRequest request, CancellationToken cancellationToken);
}

public class TencentClient : ITencentClient
{
    private readonly IMapper _mapper;
    private readonly TencentCloudSetting _tencentCloudSetting;
    private ITencentClient _tencentClientImplementation;

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
        request.RecordParams ??= new RecordParams();
        request.RecordParams.MaxIdleTime = _tencentCloudSetting.RecordingMaxIdleTime;
        request.RoomIdType = 1;
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


        request.MixTranscodeParams = new MixTranscodeParams()
        {
            VideoParams = GetVideoParams(_tencentCloudSetting.RecordingResolution)

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
    
    public VideoParams GetVideoParams(ScreenRecordingResolution resolutionLevel)
    {
        switch (resolutionLevel)
        {
            case ScreenRecordingResolution.High:
                return new VideoParams()
                {
                    Width = 1920,
                    Height = 1080,
                    Fps = 60,
                    BitRate = 3000000,
                    Gop = 10
                };

            case ScreenRecordingResolution.Medium:
                return new VideoParams()
                {
                    Width = 1280,
                    Height = 720,
                    Fps = 30,
                    BitRate = 3000000,
                    Gop = 10
                };

            case ScreenRecordingResolution.Low:
                return new VideoParams()
                {
                    Width = 640,
                    Height = 360,
                    Fps = 15,
                    BitRate = 500000,
                    Gop = 10
                };

            default:
                return new VideoParams()
                {
                    Width = 640,
                    Height = 360,
                    Fps = 15,
                    BitRate = 500000,
                    Gop = 10
                };
        }
    }

    public async Task DisbandRoomAsync(DismissRoomRequest request, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        try
        {
            Log.Information("DisbandRoomAsync request: {@request}", request);
            
            var response = await client.DismissRoom(request).ConfigureAwait(false);
            
            Log.Information("DisbandRoomAsync response: {@response}", response);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("room not exist", StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning("DismissRoomByStrRoomIdAsync: Room not exist. Request: {@request}", request);
                return;
            }
            throw;
        }
    }

    public async Task DismissRoomByStrRoomIdAsync(DismissRoomByStrRoomIdRequest request, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        
        try
        {
            Log.Information("DismissRoomByStrRoomIdRequest request: {@request}", request);
            
            var response = await client.DismissRoomByStrRoomId(request).ConfigureAwait(false);
            Log.Information("DismissRoomByStrRoomIdAsync response: {@response}", response);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("room not exist", StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning("DismissRoomByStrRoomIdAsync: Room not exist. Request: {@request}", request);
                return;
            }
            
            throw;
        }
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