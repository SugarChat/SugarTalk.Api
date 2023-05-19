using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class ScheduleMeetingCommand : ICommand
{
    public MeetingStreamMode MeetingStreamMode { get; set; } = MeetingStreamMode.MCU;
}

public class ScheduleMeetingResponse : SugarTalkResponse<ScheduleMeetingData>
{
}

public class ScheduleMeetingData
{
    public string MergedStream { get; set; }
    
    public CreateMeetingResponseDto MeetingResponse { get; set; }
}