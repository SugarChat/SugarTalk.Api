using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings.Speak;

public class GenerateChatRecordCommand : ICommand
{
    public Guid MeetingId { get; set; }
}

public class GenerateChatRecordResponse : SugarTalkResponse<MeetingSpeechDto>
{
}