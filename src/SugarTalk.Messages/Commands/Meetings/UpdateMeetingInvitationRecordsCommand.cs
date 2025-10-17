using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Commands.Meetings;

public class UpdateMeetingInvitationRecordsCommand : ICommand
{
    public List<UpdateMeetingInvitationRecordDto> MeetingInvitationRecordDtos { get; set; }
}

public class UpdateMeetingInvitationRecordsResponse : SugarTalkResponse<List<MeetingInvitationRecordDto>>
{
}

public class UpdateMeetingInvitationRecordDto
{
    public int Id { get; set; }

    public MeetingInvitationStatus InvitationStatus { get; set; }
}