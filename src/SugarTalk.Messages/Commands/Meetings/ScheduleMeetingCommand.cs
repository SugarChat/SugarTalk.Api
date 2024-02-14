using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class ScheduleMeetingCommand : AddOrUpdateMeetingDto, ICommand
{
    public bool IsLiveKit { get; set; }
}

public class ScheduleMeetingResponse : SugarTalkResponse<MeetingDto>
{
}
