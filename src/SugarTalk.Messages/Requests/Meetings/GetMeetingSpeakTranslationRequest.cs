using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Translation;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingSpeakTranslationRequest : IRequest
{
    public Guid Id { get; set; }

    public TranslationLanguage Language { get; set; }
}

public class GetMeetingSpeakTranslationResponse : SugarTalkResponse<GetMeetingSpeakTranslationDto>
{
}