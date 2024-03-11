using System;
using System.Collections.Generic;
using SugarTalk.Messages.Dto.Meetings.Speak;

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

    public List<MeetingSpeakTranslationDetailDto> MeetingSpeakTranslationDetail { get; set; }

    public string Summary { get; set; }
}