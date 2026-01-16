using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages.Requests.MeetingMonitor;

namespace SugarTalk.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class MeetingMonitorController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeetingMonitorController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [Route("konwledge"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingMonitorKnowledgeResponse))]
    public async Task<IActionResult> LoginAsync([FromQuery] GetMeetingMonitorKnowledgeRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingMonitorKnowledgeRequest, GetMeetingMonitorKnowledgeResponse>(request);

        return Ok(response);
    }
}