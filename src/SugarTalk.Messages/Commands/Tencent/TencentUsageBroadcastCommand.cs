using System;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Commands.Tencent;

public class TencentUsageBroadcastCommand : ICommand
{
    public DateTimeOffset CurrentDate { get; set; } = DateTimeOffset.Now;
}