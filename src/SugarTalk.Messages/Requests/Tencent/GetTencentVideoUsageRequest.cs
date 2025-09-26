using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Enums.Tencent;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Tencent;

public class GetTencentVideoUsageRequest : IRequest
{
    public DateTimeOffset CurrentDate { get; set; } = DateTimeOffset.Now;
}

public class GetTencentVideoUsageResponse : SugarTalkResponse<ScreenRecordingResolution?>
{
}