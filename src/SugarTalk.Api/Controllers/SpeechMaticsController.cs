using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SugarTalk.Messages.Commands.SpeechMatics;

namespace SugarTalk.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class SpeechMaticsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public SpeechMaticsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("transcription/callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TranscriptionCallbackAsync([FromBody] TranscriptionCallBackCommand command)
    {
        await _mediator.SendAsync(command).ConfigureAwait(false);

        return Ok();
    }
}