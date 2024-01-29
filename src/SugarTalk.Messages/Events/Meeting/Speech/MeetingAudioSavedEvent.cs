using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Events.Meeting.Speech;

public class MeetingAudioSavedEvent : IEvent
{
    public string Result { get; set; }
}