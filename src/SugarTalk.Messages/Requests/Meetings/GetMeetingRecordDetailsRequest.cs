using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Translation;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingRecordDetailsRequest : IRequest
{
    public Guid Id { get; set; }

    public TranslationLanguage Language { get; set; }
}

public class GetMeetingRecordDetailsResponse : SugarTalkResponse<GetMeetingRecordDetailsDto>
{
}