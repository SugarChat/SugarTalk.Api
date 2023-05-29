using System;
using System.Collections.Generic;

namespace SugarTalk.Messages.Dto.Users;

public class MeetingUserSessionDto
{
    public int Id { get; set; }
        
    public DateTimeOffset CreatedDate { get; set; }
        
    public Guid MeetingId { get; set; }
        
    public int UserId { get; set; }
    
    public string UserName { set; get; }

    public bool IsMuted { get; set; }
    
    public bool IsSharingScreen { get; set; }
}