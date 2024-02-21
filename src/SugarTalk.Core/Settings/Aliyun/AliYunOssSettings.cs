using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Aliyun;

public class AliYunOssSettings : IConfigurationSetting
{
    public AliYunOssSettings(IConfiguration configuration)
    {
        Endpoint = configuration.GetValue<string>("AliYun:Oss:Endpoint");
        BucketName = configuration.GetValue<string>("AliYun:Oss:BucketName");
        AccessKeyId = configuration.GetValue<string>("AliYun:Oss:AccessKeyId");
        AccessKeySecret = configuration.GetValue<string>("AliYun:Oss:AccessKeySecret");
    }
    
    public string Endpoint { get; set; }
    
    public string BucketName { get; set; }
    
    public string AccessKeyId { get; set; }
    
    public string AccessKeySecret { get; set; }
}