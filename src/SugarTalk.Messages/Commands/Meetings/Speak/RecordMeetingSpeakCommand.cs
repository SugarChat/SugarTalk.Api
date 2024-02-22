using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Meetings.Speak;

namespace SugarTalk.Messages.Commands.Meetings.Speak;

public class RecordMeetingSpeakCommand : ICommand
{
    public int? Id { get; set; }
    
    public string MeetingNumber { get; set; }
    
    public Guid MeetingRecordId { get; set; }
    
    public string TrackId { get; set; }
    
    public long? SpeakStartTime { get; set; }
    
    public long? SpeakEndTime { get; set; }
}

public class RecordMeetingSpeakResponse : SugarTalkResponse<MeetingSpeakDetailDto>
{
}