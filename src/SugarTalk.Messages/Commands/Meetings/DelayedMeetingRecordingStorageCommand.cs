using System;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Commands.Meetings;

public class DelayedMeetingRecordingStorageCommand : ICommand
{
    public DateTimeOffset StartDate { get; set; }
    
    public Guid MeetingId { get; set; }

    public string EgressId { get; set; }
        
    public Guid MeetingRecordId { get; set; } 
    
    public string Token { get; set; }
}