using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SugarTalk.Core.Settings.WebRTC;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MeetingController: ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IOptions<WebRtcIceServerSettings> _webRtcIceServerSettings;
        
        public MeetingController(IMediator mediator, IOptions<WebRtcIceServerSettings> webRtcIceServerSettings)
        {
            _mediator = mediator;
            _webRtcIceServerSettings = webRtcIceServerSettings;
        }
        
        [Route("schedule"), HttpPost]
        public async Task<ScheduleMeetingResponse> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand)
        {
            return await _mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(scheduleMeetingCommand);
        }
        
        [Route("join"), HttpPost]
        public async Task<JoinMeetingResponse> JoinMeeting(JoinMeetingCommand joinMeetingCommand)
        {
            return await _mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(joinMeetingCommand);
        }
        
        [Route("session"), HttpGet]
        public async Task<GetMeetingSessionResponse> GetMeetingSession([FromQuery] GetMeetingSessionRequest request)
        {
            return await _mediator.RequestAsync<GetMeetingSessionRequest, GetMeetingSessionResponse>(request);
        }
        
        [Route("iceservers"), HttpGet]
        public IActionResult GetIceServers()
        {
            return Ok(JsonConvert.DeserializeObject<WebRtcIceServer[]>(_webRtcIceServerSettings.Value.IceServers));
        }
    }
}