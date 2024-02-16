using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using Newtonsoft.Json;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Dto.Meetings.Summary;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings.Summary;

public class SummaryMeetingRecordCommand : ICommand
{
    public Guid MeetingRecordId { get; set; }
    
    public string MeetingNumber { get; set; }
    
    public List<MeetingSpeakInfoDto> SpeakInfos { get; set; }
}

public class SummaryMeetingRecordResponse : SugarTalkResponse<MeetingSummaryDto>
{
}