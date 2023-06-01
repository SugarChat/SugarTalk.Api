using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingUserSessionStreamDto
{
    public int Id { get; set; }
        
    public string StreamId { get; set; }
    
    public int MeetingUserSessionId { get; set; }

    public MeetingStreamType StreamType { get; set; }
}