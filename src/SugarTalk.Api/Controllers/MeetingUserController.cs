using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages.Commands.Meetings;
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

    [Route("setting/addOrUpdate"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddOrUpdateMeetingUserSettingResponse))]
    public async Task<IActionResult> AddOrUpdateMeetingUserSettingAsync(AddOrUpdateMeetingUserSettingCommand command)
    {
        var response = await _mediator.SendAsync<AddOrUpdateMeetingUserSettingCommand, AddOrUpdateMeetingUserSettingResponse>(command);
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
    /// 验证用户是否为会议创建人
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Route("session/master/verify"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VerifyMeetingUserPermissionResponse))]
    public async Task<IActionResult> VerifyMeetingUserPermissions(VerifyMeetingUserPermissionCommand command)
    {
        var response = await _mediator.SendAsync<VerifyMeetingUserPermissionCommand, VerifyMeetingUserPermissionResponse>(command);
        return Ok(response);
    }

    /// <summary>
    /// 会议创建人踢出指定用户
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Route("session/master/kickout"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(KickOutMeetingByUserIdResponse))]
    public async Task<IActionResult> KickOutMeeting(KickOutMeetingByUserIdCommand command)
    {
        var response = await _mediator.SendAsync<KickOutMeetingByUserIdCommand, KickOutMeetingByUserIdResponse>(command);
        return Ok(response);
    }
}