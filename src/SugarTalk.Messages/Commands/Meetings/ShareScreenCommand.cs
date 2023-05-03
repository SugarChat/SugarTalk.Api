using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class ShareScreenCommand : ICommand
{
    public int UserSessionId { get; set; }

    public bool IsShared { get; set; }
}

public class ShareScreenResponse : SugarTalkResponse<UserSessionDto>
{
}