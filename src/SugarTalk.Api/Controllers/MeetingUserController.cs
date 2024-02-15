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

    /// <summary>
    /// 验证当前用户是否为会议创建人
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