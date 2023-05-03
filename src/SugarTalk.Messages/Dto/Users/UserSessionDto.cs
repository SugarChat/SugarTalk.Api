using System;

namespace SugarTalk.Messages.Dto.Users;

public class UserSessionDto
{
    public int Id { get; set; }
        
    public DateTimeOffset CreatedDate { get; set; }
        
    public Guid MeetingId { get; set; }
        
    public string RoomStreamId { get; set; }
        
    public int UserId { get; set; }

    public bool IsMuted { get; set; }
}