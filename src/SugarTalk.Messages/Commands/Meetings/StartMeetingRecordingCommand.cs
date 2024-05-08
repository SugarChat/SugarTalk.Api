using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class StartMeetingRecordingCommand : ICommand
{
    public Guid MeetingId { get; set; }
}

public class StartMeetingRecordingResponse : SugarTalkResponse
{
    public Guid MeetingRecordId { get; set; }
    
    public string EgressId { get; set; }
}