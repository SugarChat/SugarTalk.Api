using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class CutMeetingRecordUrlCommand : ICommand
{
    public string MeetingName { get; set; }

    public string Title { get; set; }

    public string Url { get; set; }

    public Guid RecordId { get; set; }

    public Guid MeetingId { get; set; }

    public Guid? MeetingSubId { get; set; }

    public List<CutTimeDto> Times { get; set; }
}

public class CutMeetingRecordUrlResponse : SugarTalkResponse<MeetingRecordDto>
{
}

public class CutTimeDto
{
    public long StartTime { get; set; }

    public long EndTime { get; set; }
}