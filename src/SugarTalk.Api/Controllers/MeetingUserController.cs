using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Requests.Meetings.User;

namespace SugarTalk.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class MeetingUserController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeetingUserController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [Route("user/setting/addOrUpdate"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddOrUpdateMeetingUserSettingResponse))]
    public async Task<IActionResult> AddOrUpdateMeetingUserSettingAsync(AddOrUpdateMeetingUserSettingCommand command)
    {
        var response = await _mediator.SendAsync<AddOrUpdateMeetingUserSettingCommand, AddOrUpdateMeetingUserSettingResponse>(command);
            
        return Ok(response);
    }
    
    [Route("get/user/setting"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingUserSettingResponse))]
    public async Task<IActionResult> GetMeetingUserSettingAsync([FromQuery] GetMeetingUserSettingRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingUserSettingRequest, GetMeetingUserSettingResponse>(request);
            
        return Ok(response);
    }
}