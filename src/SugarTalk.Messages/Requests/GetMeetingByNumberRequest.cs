using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos;

namespace SugarTalk.Messages.Requests
{
    public class GetMeetingByNumberRequest : IRequest
    {
        public string MeetingNumber { get; set; }
    }
}