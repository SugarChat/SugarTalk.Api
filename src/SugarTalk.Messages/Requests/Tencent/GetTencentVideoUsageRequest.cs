using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using TencentCloud.Trtc.V20190722.Models;

namespace SugarTalk.Messages.Requests.Tencent;

public class GetTencentVideoUsageRequest : IRequest
{
    public DateTimeOffset CurrentDate { get; set; } = DateTimeOffset.Now;
}

public class GetTencentVideoUsageResponse : SugarTalkResponse<DescribeTrtcUsageResponse>
{
}