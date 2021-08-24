using System;
using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using SugarTalk.Core.Settings;
using SugarTalk.Messages;
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
        private readonly IOptions<WebRtcIceServerSettings> _webRtcIceServerSettings;
        
        public MeetingController(IMediator mediator, IOptions<WebRtcIceServerSettings> webRtcIceServerSettings)
        {
            _mediator = mediator;
            _webRtcIceServerSettings = webRtcIceServerSettings;
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
        
        [Route("iceservers"), HttpGet]
        public IActionResult GetIceServers()
        {
            Log.Information($"IceServers: {_webRtcIceServerSettings.Value.IceServers}");
            
            return Ok(JsonConvert.DeserializeObject<WebRtcIceServer>(_webRtcIceServerSettings.Value.IceServers));
        }
    }
}