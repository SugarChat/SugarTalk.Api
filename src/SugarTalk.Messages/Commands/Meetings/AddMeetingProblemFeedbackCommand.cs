using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class AddMeetingProblemFeedbackCommand : ICommand
{
    public MeetingProblemFeedbackDto Feedback { get; set; }
}

public class AddMeetingProblemFeedbackResponse : SugarTalkResponse<MeetingProblemFeedbackDto>
{
}