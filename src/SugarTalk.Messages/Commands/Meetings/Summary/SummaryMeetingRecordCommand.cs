using System;
using Mediator.Net.Contracts;
using System.Collections.Generic;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Translation;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Dto.Meetings.Summary;

namespace SugarTalk.Messages.Commands.Meetings.Summary;

public class SummaryMeetingRecordCommand : ICommand
{
    public Guid MeetingRecordId { get; set; }
    
    public string MeetingNumber { get; set; }

    public TranslationLanguage Language { get; set; } = TranslationLanguage.ZhCn;
    
    public List<MeetingSpeakInfoDto> SpeakInfos { get; set; }
}

public class SummaryMeetingRecordResponse : SugarTalkResponse<MeetingSummaryDto>
{
}