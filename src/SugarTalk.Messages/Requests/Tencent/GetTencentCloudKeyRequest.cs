using System;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Tencent;

public class GetTencentCloudKeyRequest : ICachingRequest
{
    public string GetCacheKey()
    {
        return $"{nameof(GetTencentCloudKeyRequest)}";
    }

    public TimeSpan? GetCacheExpiration() => TimeSpan.FromMinutes(1);
}

public class GetTencentCloudKeyResponse : SugarTalkResponse<GetTencentCloudKeyResponseData>
{
}

public class GetTencentCloudKeyResponseData
{
    public string AppId { get; set; }
    
    public string SDKSecretKey { get; set; }
}