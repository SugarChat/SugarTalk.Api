using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Translation;

namespace SugarTalk.Messages.Requests.Meetings;

public class TranslatingMeetingSpeakCommand : ICommand
{
    public Guid MeetingRecordId { get; set; }

    public TranslationLanguage Language { get; set; }
}

public class TranslatingMeetingSpeakResponse : SugarTalkResponse<GetMeetingRecordDetailsDto>
{
}