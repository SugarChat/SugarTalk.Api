using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Requests.Meetings
{
    public class GetMeetingByNumberRequest : IRequest
    {
        public string MeetingNumber { get; set; }
    }
}