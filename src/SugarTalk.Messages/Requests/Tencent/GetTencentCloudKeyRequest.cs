using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Tencent;

public class GetTencentCloudKeyRequest : IRequest
{
}

public class GetTencentCloudKeyResponse : SugarTalkResponse<GetTencentCloudKeyResponseData>
{
}

public class GetTencentCloudKeyResponseData
{
    public string AppId { get; set; }
    
    public string SDKSecretKey { get; set; }
}