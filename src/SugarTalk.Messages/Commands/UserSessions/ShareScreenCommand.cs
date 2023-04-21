using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.UserSessions;

public class ShareScreenCommand : ICommand
{
    public Guid UserSessionId { get; set; }

    public bool IsShared { get; set; }
}

public class ShareScreenResponse : SugarTalkResponse<UserSessionDto>
{
}
