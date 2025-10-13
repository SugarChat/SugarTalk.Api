using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Serilog;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Caching;
using SugarTalk.Core.Settings.TencentCloud;
using SugarTalk.Messages.Commands.Tencent;
using SugarTalk.Messages.Dto.Tencent;
using SugarTalk.Messages.Enums.Caching;
using SugarTalk.Messages.Enums.Tencent;
using SugarTalk.Messages.Requests.Tencent;
using TencentCloud.Common;
using TencentCloud.Trtc.V20190722;
using TencentCloud.Trtc.V20190722.Models;
using RecordParams = TencentCloud.Trtc.V20190722.Models.RecordParams;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ITencentClient : IScopedDependency
{
    Task<StartCloudRecordingResponse> CreateCloudRecordingAsync(CreateCloudRecordingRequest request, CancellationToken cancellationToken);

    Task<StopCloudRecordingResponse> StopCloudRecordingAsync(DeleteCloudRecordingRequest request, CancellationToken cancellationToken);

    Task<UpdateCloudRecordingResponse> ModifyCloudRecordingAsync(ModifyCloudRecordingRequest request, CancellationToken cancellationToken);

    Task DisbandRoomAsync(DismissRoomRequest request, CancellationToken cancellationToken);
    
    Task DismissRoomByStrRoomIdAsync(DismissRoomByStrRoomIdRequest request, CancellationToken cancellationToken);

    Task<GetTencentVideoUsageResponse> GetVideoUsageAsync(GetTencentVideoUsageRequest request, CancellationToken cancellationToken);

    public DescribeTrtcUsageResponse GetTencentUsageAsync(string startTime, string endTime);
}

public class TencentClient : ITencentClient
{
    private readonly IMapper _mapper;
    private readonly ICacheManager _cacheManager;
    private readonly TencentCloudSetting _tencentCloudSetting;

    public TencentClient(IMapper mapper, ICacheManager cacheManager, TencentCloudSetting tencentCloudSetting)
    {
        _mapper = mapper;
        _cacheManager = cacheManager;
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

    public async Task<GetTencentVideoUsageResponse> GetVideoUsageAsync(GetTencentVideoUsageRequest request, CancellationToken cancellationToken)
    {
        var firstDay = new DateTimeOffset(request.CurrentDate.Year, request.CurrentDate.Month, 1, 0, 0, 0, request.CurrentDate.Offset);
        
        var client = CreateClient();
        
        var req = new DescribeTrtcUsageRequest
        {
            StartTime = firstDay.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
            EndTime = request.CurrentDate.ToString("yyyy-MM-dd") + " 23:59:59",
            SdkAppId = Convert.ToUInt64(_tencentCloudSetting.AppId)
        };

        var resp = client.DescribeTrtcUsageSync(req);

        Log.Information("Tencent Meeting Usage:{@resp}", resp);

        var audio = resp.UsageList.Sum(x => Convert.ToInt32(x.UsageValue[0]));
        var SD = resp.UsageList.Sum(x => Convert.ToInt32(x.UsageValue[1]));
        var HD = resp.UsageList.Sum(x => Convert.ToInt32(x.UsageValue[2]));
        var fullHD = resp.UsageList.Sum(x => Convert.ToInt32(x.UsageValue[3]));
        
        var usageCount = audio + SD*2 + HD *4 + fullHD*9;
        var percentage = usageCount/(_tencentCloudSetting.TotalMonthlyUsage + _tencentCloudSetting.AdditionalMonthlyUsage);

        var week = GetWeekOfMonth(request.CurrentDate);
        
        var thresholds = new Dictionary<int, double>
        {
            { 1, 0.25 },
            { 2, 0.50 },
            { 3, 0.75 },
            { 4, 1.00 },
            { 5, 1.00 }
        };
        
        Log.Information($"It is currently the {week} week，used:{percentage}", week, percentage);
        
        if (thresholds.TryGetValue(week, out var threshold))
        {
            if (percentage >= threshold)
            {
                var yesterdayUsage = await _cacheManager.GetAsync<string>($"tencent-usage-{request.CurrentDate.AddDays(-1):yyyy-MM-dd}", CachingType.RedisCache, cancellationToken: cancellationToken).ConfigureAwait(false);
                
                await _cacheManager.SetAsync($"tencent-usage-{request.CurrentDate:yyyy-MM-dd}", ScreenRecordingResolution.Low.ToString(), CachingType.RedisCache, expiry: TimeSpan.FromDays(7), cancellationToken: cancellationToken).ConfigureAwait(false);

                Log.Information($"Yesterday usage:{yesterdayUsage}", yesterdayUsage);
                
                if (yesterdayUsage == "Low")
                {
                    return new GetTencentVideoUsageResponse
                    {
                        Data = ScreenRecordingResolution.Low
                    };
                }
                
                return new GetTencentVideoUsageResponse
                {
                    Data = ScreenRecordingResolution.High
                };
            }
        }
        
        return new GetTencentVideoUsageResponse
        {
            Data = ScreenRecordingResolution.High
        };
    }
    
    public DescribeTrtcUsageResponse GetTencentUsageAsync(string startTime, string endTime)
    {
        var client = CreateClient();
        
        var req = new DescribeTrtcUsageRequest
        {
            StartTime = startTime,
            EndTime = endTime,
            SdkAppId = Convert.ToUInt64(_tencentCloudSetting.AppId)
        };

        return client.DescribeTrtcUsageSync(req);
    }
    
    private static int GetWeekOfMonth(DateTimeOffset date)
    {
        var dayOfMonth = date.Day;
        return (int)Math.Ceiling(dayOfMonth / 7.0);
    }
}