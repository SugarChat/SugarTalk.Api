using System;
using System.IO;
using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using Aliyun.OSS;
using SugarTalk.Core.Settings.Aliyun;

namespace SugarTalk.Core.Services.Aliyun;

public interface IAliYunOssService : IScopedDependency
{
    string GetFileUrl(string fileName);

    string GenerateFileName(string fileName);
    
    void UploadFile(string fileName, byte[] fileContent);
    
    Task<byte[]> GetFileByteArrayAsync(string fileName, CancellationToken cancellationToken);
}

public class AliYunOssService : IAliYunOssService
{
    private readonly OssClient _ossClient;
    private readonly AliYunOssSettings _ossSettings;

    public AliYunOssService(OssClient ossClient, AliYunOssSettings ossSettings)
    {
        _ossClient = ossClient;
        _ossSettings = ossSettings;
    }

    public string GetFileUrl(string fileName)
    {
        return _ossClient.GeneratePresignedUri(_ossSettings.BucketName, fileName, DateTime.MaxValue).AbsoluteUri;
    }

    public string GenerateFileName(string fileName)
    {
        return $"{Path.GetRandomFileName()}{Path.GetExtension(fileName)}";
    }

    public void UploadFile(string fileName, byte[] fileContent)
    {
        _ossClient.PutObject(_ossSettings.BucketName, fileName, new MemoryStream(fileContent));
    }
    
    public async Task<byte[]> GetFileByteArrayAsync(string fileName, CancellationToken cancellationToken)
    {
        var file = _ossClient.GetObject(_ossSettings.BucketName, fileName);

        if (file == null) return null;

        await using var stream = new MemoryStream();
        await file.Content.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);

        return stream.ToArray();
    }
}