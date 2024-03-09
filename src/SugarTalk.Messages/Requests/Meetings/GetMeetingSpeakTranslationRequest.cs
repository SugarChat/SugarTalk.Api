using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingSpeakTranslationRequest : IRequest
{
    public Guid Id { get; set; }

    public SpeechTargetLanguageType LanguageId { get; set; }
}

public class GetMeetingSpeakTranslationResponse : SugarTalkResponse<GetMeetingSpeakTranslationDto>
{
}