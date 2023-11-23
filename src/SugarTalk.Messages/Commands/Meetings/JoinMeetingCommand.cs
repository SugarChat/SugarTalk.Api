using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class JoinMeetingCommand : ICommand
{
    public string MeetingNumber { get; set; }
    
    public string StreamId { get; set; }
    
    public MeetingStreamType StreamType { get; set; }

    public bool IsMuted { get; set; }

    public bool IsLiveKit { get; set; } = false;
}

public class JoinMeetingResponse : SugarTalkResponse<JoinMeetingResponseData>
{
}

public class JoinMeetingResponseData
{
    public MeetingDto Meeting { get; set; }
    
    public ConferenceRoomResponseBaseDto Response { get; set; }
}
