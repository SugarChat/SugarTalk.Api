using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class ScheduleMeetingCommand : ICommand
{
    public MeetingStreamMode MeetingStreamMode { get; set; }
}

public class ScheduleMeetingResponse : SugarTalkResponse<CreateMeetingResponseDto>
{
}
