using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Commands.Tencent;

namespace SugarTalk.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class TencentController : ControllerBase
{
    private readonly IMediator _mediator;

    public TencentController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [Route("cloudRecord/create"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StartCloudRecordingResponse))]
    public async Task<IActionResult> CreateCloudRecordingAsync([FromBody] CreateCloudRecordingCommand command)
    {
        var response = await _mediator.SendAsync<CreateCloudRecordingCommand, StartCloudRecordingResponse>(command);

        return Ok(response);
    }
    
    [Route("cloudRecord/stop"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StopCloudRecordingResponse))]
    public async Task<IActionResult> StopCloudRecordingAsync([FromBody] StopCloudRecordingCommand command)
    {
        var response = await _mediator.SendAsync<StopCloudRecordingCommand, StopCloudRecordingResponse>(command);

        return Ok(response);
    }
    
    
    [Route("cloudRecord/update"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateCloudRecordingResponse))]
    public async Task<IActionResult> UpdateCloudRecordingAsync([FromBody] UpdateCloudRecordingCommand command)
    {
        var response = await _mediator.SendAsync<UpdateCloudRecordingCommand, UpdateCloudRecordingResponse>(command);

        return Ok(response);
    }
}