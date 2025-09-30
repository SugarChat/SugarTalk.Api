using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Enums.Tencent;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Tencent;

[AllowGuestAccess]
public class GetTencentVideoUsageRequest : IRequest
{
    public DateTimeOffset CurrentDate { get; set; } = DateTimeOffset.Now;
}

public class GetTencentVideoUsageResponse : SugarTalkResponse<ScreenRecordingResolution?>
{
}