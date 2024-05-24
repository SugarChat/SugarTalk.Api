using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings.Speech;

namespace SugarTalk.Messages.Events.Meeting.Speech;

public class GetMeetingChatVoiceRecordEvent : IEvent
{
    public List<MeetingSpeechDto> MeetingSpeech { get; set; }
    
    /*public List<MeetingChatVoiceRecordDto> ShouldGenerateVoiceRecords { get; set; }*/
}