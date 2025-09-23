using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages.Commands.Tencent;
using SugarTalk.Messages.Requests.Tencent;

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

    [Route("cloud/key"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetTencentCloudKeyResponse))]
    public async Task<IActionResult> GetTencentCloudKey()
    {
        var response = await _mediator.RequestAsync<GetTencentCloudKeyRequest, GetTencentCloudKeyResponse>(new GetTencentCloudKeyRequest());
        
        return Ok(response);
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
    
    [AllowAnonymous]
    [Route("cloudRecord/callback"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CloudRecordingCallBackAsync([FromBody] CloudRecordingCallBackCommand command)
    {
        await _mediator.SendAsync(command);

        return Ok();
    }
    
    [Route("cloudRecord/get"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetTencentVideoUsageResponse))]
    public async Task<IActionResult> GetTencentVideoUsageAsync([FromQuery] GetTencentVideoUsageRequest request)
    {
        var response = await _mediator.RequestAsync<GetTencentVideoUsageRequest, GetTencentVideoUsageResponse>(request);

        return Ok(response);
    }
}