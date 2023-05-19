using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class EndMeetingCommand : ICommand
{
    public string MeetingNumber { get; set; }
}

public class EndMeetingResponse : SugarTalkResponse<EndMeetingData>
{
}

public class EndMeetingData
{
    public string MeetingNumber { get; set; }
    
    public List<int> MeetingUserSessionIds { get; set; }
}