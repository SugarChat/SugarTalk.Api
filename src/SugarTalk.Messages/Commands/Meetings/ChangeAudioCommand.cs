using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class ChangeAudioCommand : ICommand
{
    public string AccessToken { get; set; }

    public bool IsMuted { get; set; }
}

public class ChangeAudioResponse : SugarTalkResponse<MeetingUserSessionDto>
{
}