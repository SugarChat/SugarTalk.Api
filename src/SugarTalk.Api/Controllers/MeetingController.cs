using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MeetingController: ControllerBase
    {
        private readonly IMediator _mediator;
        
        public MeetingController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [Route("schedule"), HttpPost]
        public async Task<SugarTalkResponse<MeetingDto>> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand)
        {
            return await _mediator.SendAsync<ScheduleMeetingCommand, SugarTalkResponse<MeetingDto>>(scheduleMeetingCommand);
        }
        
        [Route("join"), HttpPost]
        public async Task<SugarTalkResponse<MeetingSessionDto>> JoinMeeting(JoinMeetingCommand joinMeetingCommand)
        {
            return await _mediator.SendAsync<JoinMeetingCommand, SugarTalkResponse<MeetingSessionDto>>(joinMeetingCommand);
        }
        
        [Route("session"), HttpGet]
        public async Task<SugarTalkResponse<MeetingSessionDto>> GetMeetingSession([FromQuery] GetMeetingSessionRequest request)
        {
            return await _mediator.RequestAsync<GetMeetingSessionRequest, SugarTalkResponse<MeetingSessionDto>>(request);
        }
    }
}