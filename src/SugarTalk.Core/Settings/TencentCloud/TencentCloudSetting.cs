using System;
using Microsoft.Extensions.Configuration;
using SugarTalk.Messages.Enums.Tencent;

namespace SugarTalk.Core.Settings.TencentCloud;

public class TencentCloudSetting: IConfigurationSetting
{
    public TencentCloudSetting(IConfiguration configuration)
    {
        AppId = configuration.GetValue<string>("Tencent:AppId");
        SDKSecretKey = configuration.GetValue<string>("Tencent:SDKSecretKey");
        SecretId = configuration.GetValue<string>("Tencent:SecretId");
        SecretKey = configuration.GetValue<string>("Tencent:SecretKey");
        Region = configuration.GetValue<string>("Tencent:Region");
        CosBucket = configuration.GetValue<string>("Tencent:StorageParams:Bucket");
        CosBaseUrl = configuration.GetValue<string>("Tencent:StorageParams:CosBaseUrl");
        CosFileNamePrefix = configuration.GetValue<string>("Tencent:StorageParams:FileNamePrefix");
        RecordingMaxIdleTime = configuration.GetValue<ulong>("Tencent:RecordParams:MaxIdleTime");
        RecordingResolution = Enum.Parse<ScreenRecordingResolution>(configuration.GetValue<string>("Tencent:RecordingResolution"), true);
        TotalMonthlyUsage = configuration.GetValue<double>("Tencent:TotalMonthlyUsage");
        AdditionalMonthlyUsage = configuration.GetValue<double>("Tencent:AdditionalMonthlyUsage");
        
        RobotUrl = configuration.GetValue<string>("Tencent:RobotUrl");
    }
    
    public string AppId { get; set; }
    
    public string SDKSecretKey { get; set; }
    
    public string SecretId { get; set; }
    
    public string SecretKey { get; set; }
    
    public string Region { get; set; }
    
    public string CosBucket { get; set; }

    public string CosBaseUrl { get; set; }
    
    public string CosFileNamePrefix { get; set; }
    
    public ulong RecordingMaxIdleTime { get; set; }
    
    public ScreenRecordingResolution RecordingResolution { get; set; }

    public double TotalMonthlyUsage { get; set; }

    public double AdditionalMonthlyUsage { get; set; }

    public string RobotUrl { get; set; }
}