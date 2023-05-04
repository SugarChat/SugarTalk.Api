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
    public async Task<IActionResult> JoinMeetingAsync([FromBody] JoinMeetingCommand joinMeetingCommand)
    {
        var response = await _mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(joinMeetingCommand);

        return Ok(response);
    }
    
    [Route("screen/share"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShareScreenResponse))]
    public async Task<IActionResult> ShareScreenAsync(ShareScreenCommand shareScreenCommand)
    {
        var response = await _mediator.SendAsync<ShareScreenCommand, ShareScreenResponse>(shareScreenCommand);

        return Ok(response);
    }
    
    [Route("audio/change"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChangeAudioResponse))]
    public async Task<IActionResult> ChangeAudioAsync(ChangeAudioCommand changeAudioCommand)
    {
        var response = await _mediator.SendAsync<ChangeAudioCommand, ChangeAudioResponse>(changeAudioCommand);

        return Ok(response);
    }
}