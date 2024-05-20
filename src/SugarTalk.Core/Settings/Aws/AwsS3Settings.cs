using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Aws;

public class AwsS3Settings : IConfigurationSetting
{
    public AwsS3Settings(IConfiguration configuration)
    {
        Endpoint = configuration.GetValue<string>("Aws:S3:Endpoint");
        BucketName = configuration.GetValue<string>("Aws:S3:BucketName");
        AccessKeyId = configuration.GetValue<string>("Aws:S3:AccessKeyId");
        AccessKeySecret = configuration.GetValue<string>("Aws:S3:AccessKeySecret");
    }

    public string Endpoint { get; set; }

    public string BucketName { get; set; }

    public string AccessKeyId { get; set; }

    public string AccessKeySecret { get; set; }
}