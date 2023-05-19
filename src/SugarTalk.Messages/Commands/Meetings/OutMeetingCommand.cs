using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class OutMeetingCommand : ICommand
{
    public Guid MeetingId { get; set; }
    
    public string StreamId { get; set; }
}

public class OutMeetingResponse : SugarTalkResponse<OutMeetingData>
{
}

public class OutMeetingData
{
    public bool IsOuted { get; set; }
    
    public string MergedStream { get; set; }
}