using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingOnlineLongestDurationUserRequest : IRequest
{
    public Guid MeetingId { get; set; }
}

public class GetMeetingOnlineLongestDurationUserResponse : SugarTalkResponse<MeetingOnlineLongestDurationUserDto>
{
}

public class MeetingOnlineLongestDurationUserDto
{
    public int UserId { get; set; }
    
    public string UserName { get; set; }

    public bool CoHost { get; set; }

    public bool IsMeetingMaster { get; set; }
}