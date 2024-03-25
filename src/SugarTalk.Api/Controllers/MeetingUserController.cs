using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Commands.Meetings.Speak;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Dto.Meetings.Speech;
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

    [Route("setting/addOrUpdate"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddOrUpdateMeetingUserSettingResponse))]
    public async Task<IActionResult> AddOrUpdateMeetingUserSettingAsync(AddOrUpdateMeetingUserSettingCommand command)
    {
        var response = await _mediator.SendAsync<AddOrUpdateMeetingUserSettingCommand, AddOrUpdateMeetingUserSettingResponse>(command);
        
        return Ok(response);
    }
    
    [Route("setting/chatRoom/addOrUpdate"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddOrUpdateChatRoomSettingResponse))]
    public async Task<IActionResult> AddOrUpdateChatRoomSettingAsync(AddOrUpdateChatRoomSettingCommand command)
    {
        var response = await _mediator.SendAsync<AddOrUpdateChatRoomSettingCommand, AddOrUpdateChatRoomSettingResponse>(command).ConfigureAwait(false);
        
        return Ok(response);
    }

    [Route("setting"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingUserSettingResponse))]
    public async Task<IActionResult> GetMeetingUserSettingAsync([FromQuery] GetMeetingUserSettingRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingUserSettingRequest, GetMeetingUserSettingResponse>(request);
        
        return Ok(response);
    }

    /// <summary>
    /// verify user permission in meeting
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Route("session/master/verify"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VerifyMeetingUserPermissionResponse))]
    public async Task<IActionResult> VerifyMeetingUserPermissionsAsync(VerifyMeetingUserPermissionCommand command)
    {
        var response = await _mediator.SendAsync<VerifyMeetingUserPermissionCommand, VerifyMeetingUserPermissionResponse>(command).ConfigureAwait(false);
        
        return Ok(response);
    }

    /// <summary>
    /// kick out user from meeting
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Route("session/master/kickout"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(KickOutMeetingByUserIdResponse))]
    public async Task<IActionResult> KickOutMeetingAsync(KickOutMeetingByUserIdCommand command)
    {
        var response = await _mediator.SendAsync<KickOutMeetingByUserIdCommand, KickOutMeetingByUserIdResponse>(command).ConfigureAwait(false);
        
        return Ok(response);
    }
}