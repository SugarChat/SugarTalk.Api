using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class VerifyMeetingUserPermissionCommand : ICommand
{
    /// <summary>
    /// 会议Id
    /// </summary>
    public Guid MeetingId { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public int UserId { get; set; }
}

public class VerifyMeetingUserPermissionResponse : SugarTalkResponse<VerifyMeetingUserPermissionDto>
{
}