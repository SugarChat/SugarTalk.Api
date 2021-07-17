using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Requests.Meetings
{
    public class GetMeetingSessionRequest : IRequest
    {
        public string MeetingNumber { get; set; }
    }
}