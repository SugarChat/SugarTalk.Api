using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.Aws;

namespace SugarTalk.Core.Services.Aws;

public interface IAwsS3Service : IScopedDependency
{
    string GetFileUrl(string fileName);
    
    Task UploadFileAsync(string fileName, byte[] fileContent, CancellationToken cancellationToken);
}

public class AwsS3Service : IAwsS3Service
{
    private readonly AmazonS3Client _amazonS3;
    private readonly AwsS3Settings _awsOssSettings;

    public AwsS3Service(AwsS3Settings awsOssSettings, AmazonS3Client amazonS3)
    {
        _amazonS3 = amazonS3;
        _awsOssSettings = awsOssSettings;
    }
    
    public string GetFileUrl(string fileName)
    {
        return $"https://{_awsOssSettings.BucketName}.s3.{_awsOssSettings.Endpoint}.amazonaws.com/{fileName}";
    }

    public async Task UploadFileAsync(string fileName, byte[] fileContent, CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            Key = fileName,
            BucketName = _awsOssSettings.BucketName,
            InputStream = new MemoryStream(fileContent),
            CannedACL = S3CannedACL.PublicRead
        };
        
        await _amazonS3.PutObjectAsync(request, cancellationToken);
    }
}