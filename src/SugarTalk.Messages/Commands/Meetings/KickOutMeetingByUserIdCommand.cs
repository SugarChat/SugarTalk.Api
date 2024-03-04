using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class KickOutMeetingByUserIdCommand : ICommand
{
    /// <summary>
    /// 会议Id
    /// </summary>
    public Guid MeetingId { get; set; }

    /// <summary>
    /// 需要踢出的用户Id
    /// </summary>
    public int KickOutUserId { get; set; }
}

public class KickOutMeetingByUserIdResponse : SugarTalkResponse<MeetingDto>
{
}