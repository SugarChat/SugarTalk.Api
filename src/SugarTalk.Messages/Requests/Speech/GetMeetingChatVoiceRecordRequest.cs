using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Speech;

public class GetMeetingChatVoiceRecordRequest : IRequest
{
    public Guid MeetingId { get; set; }

    //我听到的语种设置
    public SpeechTargetLanguageType LanguageType { get; set; } = SpeechTargetLanguageType.Cantonese;
}

public class GetMeetingChatVoiceRecordResponse : SugarTalkResponse<List<MeetingChatVoiceRecordDto>>
{
}