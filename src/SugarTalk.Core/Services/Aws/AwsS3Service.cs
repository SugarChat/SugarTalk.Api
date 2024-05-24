using System;
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
    
    Task<byte[]> GetFileStreamAsync(string fileName, CancellationToken cancellationToken = default);

    Task<string> GeneratePresignedUrlAsync(string fileName, double durationInMinutes = 1);
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
    
    public async Task<byte[]> GetFileStreamAsync(string fileName, CancellationToken cancellationToken = default) 
    {
        var request = new GetObjectRequest
        {
            BucketName = _awsOssSettings.BucketName,
            Key = fileName
        };

        using (GetObjectResponse response = await _amazonS3.GetObjectAsync(request, cancellationToken).ConfigureAwait(false))
        using (MemoryStream memoryStream = new MemoryStream())
        {
            await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            byte[] byteArray = memoryStream.ToArray();
            return byteArray;
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(string fileName, double durationInMinutes = 1)
    {
        var request = new GetPreSignedUrlRequest
        {
            Key = fileName,
            BucketName = _awsOssSettings.BucketName,
            Expires = DateTime.UtcNow.AddMinutes(durationInMinutes)
        };
        
        return await _amazonS3.GetPreSignedURLAsync(request).ConfigureAwait(false);
    }
}