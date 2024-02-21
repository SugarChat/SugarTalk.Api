using System;
using System.Collections.Generic;

namespace SugarTalk.Messages.Dto.Meetings;

public class GetMeetingRecordDetailsDto
{
    public Guid Id { get; set; }
   
    public string MeetingTitle { get; set; }
   
    public string MeetingNumber { get; set; }
   
    public long MeetingStartDate { get; set; }
   
    public long MeetingEndDate { get; set; }
    
    public string Url { get; set; }

    public List<MeetingRecordDetail> MeetingRecordDetail { get; set; }
   
    public string Summary { get; set; }
}

public class MeetingRecordDetail
{
    public int UserId { get; set; }
   
    public long SpeakStartTime { get; set; }
   
    public string SpeakContent { get; set; }
}