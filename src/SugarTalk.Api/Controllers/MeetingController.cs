using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class MeetingController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeetingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Route("schedule"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ScheduleMeetingResponse))]
    public async Task<IActionResult> ScheduleMeetingAsync([FromBody] ScheduleMeetingCommand scheduleMeetingCommand)
    {
        var response =
            await _mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(scheduleMeetingCommand);

        return Ok(response);
    }

    [Route("join"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JoinMeetingResponse))]
    public async Task<IActionResult> JoinMeetingAsync([FromBody] JoinMeetingCommand command)
    {
        var response = await _mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(command);

        return Ok(response);
    }
    
    [Route("out"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OutMeetingResponse))]
    public async Task<IActionResult> OutMeetingAsync([FromBody] OutMeetingCommand command)
    {
        var response = await _mediator.SendAsync<OutMeetingCommand, OutMeetingResponse>(command);

        return Ok(response);
    }
    
    [Route("end"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EndMeetingResponse))]
    public async Task<IActionResult> EndMeetingAsync([FromBody] EndMeetingCommand command)
    {
        var response = await _mediator.SendAsync<EndMeetingCommand, EndMeetingResponse>(command);

        return Ok(response);
    }
    
    [Route("get"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingByNumberResponse))]
    public async Task<IActionResult> GetMeetingByNumberAsync([FromQuery] GetMeetingByNumberRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(request);

        return Ok(response);
    }

    [AllowAnonymous]
    [Route("simple/get"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetSimpleMeetingResponse))]
    public async Task<IActionResult> GetSimpleMeetingAsync([FromQuery] GetSimpleMeetingRequest request)
    {
        var response = await _mediator.RequestAsync<GetSimpleMeetingRequest, GetSimpleMeetingResponse>(request);

        return Ok(response);
    }
    
    [Route("screen/share"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShareScreenResponse))]
    public async Task<IActionResult> ShareScreenAsync(ShareScreenCommand command)
    {
        var response = await _mediator.SendAsync<ShareScreenCommand, ShareScreenResponse>(command);

        return Ok(response);
    }
    
    [Route("audio/change"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChangeAudioResponse))]
    public async Task<IActionResult> ChangeAudioAsync(ChangeAudioCommand command)
    {
        var response = await _mediator.SendAsync<ChangeAudioCommand, ChangeAudioResponse>(command);

        return Ok(response);
    }
}