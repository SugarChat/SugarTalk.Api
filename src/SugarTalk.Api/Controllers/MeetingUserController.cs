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

//[Authorize]
[ApiController]
[Route("[controller]")]
public class MeetingUserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SpeechClient _speechClient;

    public MeetingUserController(IMediator mediator, SpeechClient speechClient)
    {
        _mediator = mediator;
        _speechClient = speechClient;
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
    
    [Route("speak"), HttpPost]
    public async Task<IActionResult> SpeakAsync()
    {
        var response = await _speechClient.SpeechToInferenceMandarinAsync(new SpeechToInferenceMandarinDto
        {
            VoiceId = "6f4d0fb7-ab21-4749-910a-9ce894a45a5c",
            UserName = "钮哥的音色",
            Text = "你好嗎你好嗎你好嗎\n"
        }, CancellationToken.None).ConfigureAwait(false);
        
        return Ok(response);
    }
}