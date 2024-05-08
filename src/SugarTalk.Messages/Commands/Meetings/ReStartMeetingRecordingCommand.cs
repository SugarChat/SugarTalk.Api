using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Messages.Commands.Meetings;

public class ReStartMeetingRecordingCommand : ICommand
{
    public Guid MeetingId { get; set; }
    
    public  UserAccountDto User { get; set; }
    
    public Guid MeetingRecordId { get; set; }
    
    public Guid MeetingRestartRecordId { get; set; }
    
    public bool IsRestartRecord { get; set; } = false;
}

public class ReStartMeetingRecordingResponse : IResponse
{
}