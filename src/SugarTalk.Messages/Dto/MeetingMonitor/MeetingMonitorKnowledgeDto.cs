using System;

namespace SugarTalk.Messages.Dto.MeetingMonitor;

public class MeetingMonitorKnowledgeDto
{
    public int Id { get; set; }
    
    public string Prompt { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
}