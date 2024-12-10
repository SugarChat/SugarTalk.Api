using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Messages.Events.Meeting;

public class AddMeetingProblemFeedbackEvent : IEvent
{
    public MeetingProblemFeedbackDto Feedback { get; set; }
}