using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Events.Meeting.Speech;

public class MeetingSpeechUpdatedEvent : IEvent
{
    public string Result { get; set; }
}