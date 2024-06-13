using Serilog;
using System;
using Amazon.S3;
using System.IO;
using Amazon.S3.Model;
using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
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
        return $"https://{_awsOssSettings.BucketName}.{_awsOssSettings.Endpoint}/{fileName}";
    }

    public async Task UploadFileAsync(string fileName, byte[] fileContent, CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            Key = fileName,
            BucketName = _awsOssSettings.BucketName,
            InputStream = new MemoryStream(fileContent)
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

        using var response = await _amazonS3.GetObjectAsync(request, cancellationToken).ConfigureAwait(false);
        
        Log.Information($"Start reading stream");
        
        using var memoryStream = new MemoryStream();
        var buffer = new byte[81920];
        int bytesRead;

        while ((bytesRead = await response.ResponseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
        }

        return memoryStream.ToArray();
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