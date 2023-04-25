using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class ScheduleMeetingCommand : ICommand
{
    public MeetingType MeetingType { get; set; }
    
    public StreamMode MeetingMode { get; set; }
}

public class ScheduleMeetingResponse : SugarTalkResponse<CreateMeetingResponseDto>
{
}
