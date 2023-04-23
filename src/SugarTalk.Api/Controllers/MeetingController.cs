using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Settings.WebRTC;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dtos.AntMedia;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class MeetingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAntMediaClient _antMediaClient;
    private readonly WebRtcIceServerSettings _webRtcIceServerSettings;

    public MeetingController(IMediator mediator, IAntMediaClient antMediaClient, WebRtcIceServerSettings webRtcIceServerSettings)
    {
        _mediator = mediator;
        _antMediaClient = antMediaClient;
        _webRtcIceServerSettings = webRtcIceServerSettings;
    }

    [Route("schedule"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ScheduleMeetingResponse))]
    public async Task<IActionResult> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand)
    {
        var response =
            await _mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(scheduleMeetingCommand);

        return Ok(response);
    }

    [Route("join"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JoinMeetingResponse))]
    public async Task<IActionResult> JoinMeeting(JoinMeetingCommand joinMeetingCommand)
    {
        var response = await _mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(joinMeetingCommand);

        return Ok(response);
    }

    [Route("session"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingSessionResponse))]
    public async Task<IActionResult> GetMeetingSession([FromQuery] GetMeetingSessionRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingSessionRequest, GetMeetingSessionResponse>(request);

        return Ok(response);
    }

    [Route("test"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> test(string roomId)
    {
        return Ok(await _antMediaClient.GetAntMediaConferenceRoomAsync(roomId, CancellationToken.None).ConfigureAwait(false));
    }
    
    [Route("test2"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> test2(int size)
    {
        return Ok(await _antMediaClient.GetAntMediaConferenceRoomsAsync(0, size, CancellationToken.None).ConfigureAwait(false));
    }
    
    [Route("test3"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> test3(string roomInfoId)
    {
        return Ok(await _antMediaClient.GetAntMediaConferenceRoomInfoAsync(roomInfoId, CancellationToken.None).ConfigureAwait(false));
    }
    
    [Route("test4"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> test4(GetAntMediaConferenceRoomResponseDto roomData)
    {
        return Ok(await _antMediaClient.CreateAntMediaConferenceRoomAsync(roomData, CancellationToken.None).ConfigureAwait(false));
    }

    [Route("iceservers"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetIceServers()
    {
        return Ok(JsonConvert.DeserializeObject<WebRtcIceServer[]>(_webRtcIceServerSettings.IceServers));
    }
}