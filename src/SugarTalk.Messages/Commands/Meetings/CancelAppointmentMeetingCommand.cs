using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class CancelAppointmentMeetingCommand : ICommand
{
    public Guid MeetingId { get; set; }
}

public class CancelAppointmentMeetingResponse : SugarTalkResponse
{
}
