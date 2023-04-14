using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.UserSessions;

public class ChangeAudioCommand : ICommand
{
    public Guid UserSessionId { get; set; }
    public bool IsMuted { get; set; }
}

public class ChangeAudioResponse : SugarTalkResponse<UserSessionDto>
{
}
