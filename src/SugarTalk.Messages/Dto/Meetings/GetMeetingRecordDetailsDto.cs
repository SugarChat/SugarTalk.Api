using System;
using System.Collections.Generic;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Dto.Meetings.Summary;

namespace SugarTalk.Messages.Dto.Meetings;

public class GetMeetingRecordDetailsDto
{
    public Guid Id { get; set; }
   
    public string MeetingTitle { get; set; }
   
    public string MeetingNumber { get; set; }
   
    public long MeetingStartDate { get; set; }
   
    public long MeetingEndDate { get; set; }
    
    public string Url { get; set; }

    public List<MeetingSpeakDetailDto> MeetingRecordDetail { get; set; }

    public MeetingSummaryDto Summary { get; set; }
}

public class GetMeetingRecordDetailsMappingDto
{
    public Guid Id { get; set; }
   
    public string MeetingTitle { get; set; }
   
    public string MeetingNumber { get; set; }
   
    public long MeetingStartDate { get; set; }
   
    public long MeetingEndDate { get; set; }
    
    public string Url { get; set; }

    public List<MeetingSpeakDetailDto> MeetingRecordDetail { get; set; }
    
    public MeetingSummary Summary { get; set; }
}