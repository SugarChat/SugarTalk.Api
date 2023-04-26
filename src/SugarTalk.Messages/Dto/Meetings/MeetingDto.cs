using System;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingDto
{
    public Guid Id { get; set; }

    public MeetingStreamMode MeetingMode { get; set; }

    public string MeetingNumber { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }

    public List<string> RoomStreamList { get; set; }

    public string Mode { get; set; }

    public string OriginAdress { get; set; }
}
