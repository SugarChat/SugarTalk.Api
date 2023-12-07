using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Requests.Speech;

namespace SugarTalk.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class MeetingSpeechController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeetingSpeechController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [Route("save/audio"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SaveMeetingAudioResponse))]
    public async Task<IActionResult> SaveMeetingAudioAsync(SaveMeetingAudioCommand command)
    {
        var response = await _mediator.SendAsync<SaveMeetingAudioCommand, SaveMeetingAudioResponse>(command);
            
        return Ok(response);
    }
    
    [Route("update"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateMeetingSpeechResponse))]
    public async Task<IActionResult> UpdateMeetingSpeechAsync(UpdateMeetingSpeechCommand command)
    {
        var response = await _mediator.SendAsync<UpdateMeetingSpeechCommand, UpdateMeetingSpeechResponse>(command);
            
        return Ok(response);
    }
    
    [Route("list"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingAudioListResponse))]
    public async Task<IActionResult> GetMeetingAudioListAsync([FromQuery] GetMeetingAudioListRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingAudioListRequest, GetMeetingAudioListResponse>(request);
            
        return Ok(response);
    }
}