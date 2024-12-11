using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Events.Meeting;

public class MeetingProblemFeedbackAddedEvent : IEvent
{
    public MeetingProblemFeedbackDto Feedback { get; set; }
}