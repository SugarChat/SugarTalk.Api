using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.TencentCloud;

public class TencentCloudSetting: IConfigurationSetting
{
    public TencentCloudSetting(IConfiguration configuration)
    {
        BaseUrl = configuration.GetValue<string>("Tencent:BaseUrl");
        SecretId = configuration.GetValue<string>("Tencent:SecretId");
        SecretKey = configuration.GetValue<string>("Tencent:SecretKey");
        Region = configuration.GetValue<string>("Tencent:Region");
        
    }

    public string BaseUrl { get; set; }
    
    public string SecretId { get; set; }
    
    public string SecretKey { get; set; }
    
    public string Region { get; set; }
}