using Microsoft.Extensions.Configuration;

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
        CosRegion =  configuration.GetValue<string>("Tencent:StorageParams:Region");
        CosBucket =  configuration.GetValue<string>("Tencent:StorageParams:Bucket");
        CosAccessKey =  configuration.GetValue<string>("Tencent:StorageParams:AccessKey");
        CosSecretKey =  configuration.GetValue<string>("Tencent:StorageParams:SecretKey");
    }
    
    public string AppId { get; set; }
    
    public string SDKSecretKey { get; set; }
    
    public string SecretId { get; set; }
    
    public string SecretKey { get; set; }
    
    public string Region { get; set; }
    
    public string CosRegion { get; set; }
    
    public string CosBucket { get; set; }
    
    public string CosAccessKey { get; set; }
    
    public string CosSecretKey { get; set; }
}