using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class UpdateMeetingCommand : MeetingBaseDto, ICommand
{
    public string SecurityCode { get; set;}
}

public class UpdateMeetingResponse : SugarTalkResponse
{
}