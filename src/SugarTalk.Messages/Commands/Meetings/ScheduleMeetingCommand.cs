using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class ScheduleMeetingCommand : ICommand
{
    public MeetingStreamMode MeetingStreamMode { get; set; } = MeetingStreamMode.MCU;
    
    public DateTimeOffset StartDate { get; set; }
    
    public DateTimeOffset EndDate { get; set; }
}

public class ScheduleMeetingResponse : SugarTalkResponse<MeetingDto>
{
}
