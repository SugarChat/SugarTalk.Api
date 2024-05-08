using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Messages.Commands.Meetings;

public class GeneralRestartRecordCommand : ICommand
{
    public Guid MeetingId { get; set; }

    public UserAccountDto User { get; set; }

    public Guid MeetingRecordId { get; set; }
    
    public int ReTryLimit { get; set; } = 5;
}