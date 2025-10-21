using System.Collections.Generic;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Commands.Meetings;

public class GeneralMeetingInvitationRecordStatusCommand : ICommand
{
    public List<int> Ids { get; set; }
}