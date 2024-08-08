using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Messages.Commands.Meetings;

public class DelayedMeetingRecordingStorageCommand : ICommand
{
    public DateTimeOffset StartDate { get; set; }
    
    public Guid MeetingId { get; set; }

    public string EgressId { get; set; }
        
    public Guid MeetingRecordId { get; set; } 
    
    public string Token { get; set; }

    public int ReTryLimit { get; set; } = 5;

    public bool IsRestartRecord { get; set; } = false;

    public UserAccountDto User { get; set; }
}