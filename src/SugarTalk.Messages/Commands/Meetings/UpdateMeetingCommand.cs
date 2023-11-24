using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class UpdateMeetingCommand : UpdateMeetingDto, ICommand
{
}

public class UpdateMeetingResponse : SugarTalkResponse
{
}