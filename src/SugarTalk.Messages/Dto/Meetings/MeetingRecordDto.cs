using System;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingRecordDto
{
    public Guid MeetingRecordId { get; set; }
    
    public Guid MeetingId { get; set; }

    public string MeetingNumber { get; set; }

    public string RecordNumber { get; set; }

    public string Title { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }

    public long Duration { get; set; }

    public string Timezone { get; set; }

    public string MeetingCreator { get; set; }

    public string Url { get; set; }
}