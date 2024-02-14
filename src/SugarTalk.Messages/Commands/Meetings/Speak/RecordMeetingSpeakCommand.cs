using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Meetings.Speak;

namespace SugarTalk.Messages.Commands.Meetings.Speak;

public class RecordMeetingSpeakCommand : ICommand
{
    public Guid? Id { get; set; }
    
    public Guid MeetingId { get; set; }
    
    public Guid? MeetingSubId { get; set; }
    
    public long? SpeakStartTime { get; set; }
    
    public long? SpeakEndTime { get; set; }
}

public class RecordMeetingSpeakResponse : SugarTalkResponse<MeetingSpeakDetailDto>
{
}