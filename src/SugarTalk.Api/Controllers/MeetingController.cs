using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Commands.Meetings.Speak;

namespace SugarTalk.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class MeetingController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeetingController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [Route("schedule"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ScheduleMeetingResponse))]
    public async Task<IActionResult> ScheduleMeetingAsync([FromBody] ScheduleMeetingCommand scheduleMeetingCommand)
    {
        var response =
            await _mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(scheduleMeetingCommand).ConfigureAwait(false);

        return Ok(response);
    }

    [Route("join"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JoinMeetingResponse))]
    public async Task<IActionResult> JoinMeetingAsync([FromBody] JoinMeetingCommand command)
    {
        var response = await _mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("out"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OutMeetingResponse))]
    public async Task<IActionResult> OutMeetingAsync([FromBody] OutMeetingCommand command)
    {
        var response = await _mediator.SendAsync<OutMeetingCommand, OutMeetingResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("update"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateMeetingResponse))]
    public async Task<IActionResult> UpdateMeetingAsync([FromBody] UpdateMeetingCommand command)
    {
        var response = await _mediator.SendAsync<UpdateMeetingCommand, UpdateMeetingResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [Route("end"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EndMeetingResponse))]
    public async Task<IActionResult> EndMeetingAsync([FromBody] EndMeetingCommand command)
    {
        var response = await _mediator.SendAsync<EndMeetingCommand, EndMeetingResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("get"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingByNumberResponse))]
    public async Task<IActionResult> GetMeetingByNumberAsync([FromQuery] GetMeetingByNumberRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("get/userSession/list"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingUserSessionsResponse))]
    public async Task<IActionResult> GetMeetingUserSessionsAsync([FromQuery] GetMeetingUserSessionsRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingUserSessionsRequest, GetMeetingUserSessionsResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("get/userSession/{userId}"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingUserSessionByUserIdResponse))]
    public async Task<IActionResult> GetMeetingUserSessionByUserIdAsync(int userId)
    {
        var response =
            await _mediator.RequestAsync<GetMeetingUserSessionByUserIdRequest, GetMeetingUserSessionByUserIdResponse>(
                new GetMeetingUserSessionByUserIdRequest { UserId = userId }).ConfigureAwait(false);

        return Ok(response);
    }

    [Route("screen/share"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShareScreenResponse))]
    public async Task<IActionResult> ShareScreenAsync(ShareScreenCommand command)
    {
        var response = await _mediator.SendAsync<ShareScreenCommand, ShareScreenResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("audio/change"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChangeAudioResponse))]
    public async Task<IActionResult> ChangeAudioAsync(ChangeAudioCommand command)
    {
        var response = await _mediator.SendAsync<ChangeAudioCommand, ChangeAudioResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [Route("history"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingHistoriesByUserResponse))]
    public async Task<IActionResult> GetMeetingHistoriesByUserAsync([FromQuery] GetMeetingHistoriesByUserRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingHistoriesByUserRequest, GetMeetingHistoriesByUserResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("appointment"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAppointmentMeetingsResponse))]
    public async Task<IActionResult> GetAppointmentMeetingsAsync([FromQuery] GetAppointmentMeetingsRequest request)
    {
        var response = await _mediator.RequestAsync<GetAppointmentMeetingsRequest, GetAppointmentMeetingsResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }

    [Route("record"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetCurrentUserMeetingRecordResponse))]
    public async Task<IActionResult> GetCurrentUserMeetingRecordAsync([FromQuery] GetCurrentUserMeetingRecordRequest request)
    {
        var response = await _mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }

    #region Speak Detail

    [Route("record/speak"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RecordMeetingSpeakResponse))]
    public async Task<IActionResult> RecordMeetingSpeakAsync([FromBody] RecordMeetingSpeakCommand command)
    {
        var response = await _mediator.SendAsync<RecordMeetingSpeakCommand, RecordMeetingSpeakResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    #endregion
    
    [Route("history/delete"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeleteMeetingHistoryResponse))]
    public async Task<IActionResult> DeleteMeetingHistoryAsync([FromBody] DeleteMeetingHistoryCommand command)
    {
        var response = await _mediator.SendAsync<DeleteMeetingHistoryCommand, DeleteMeetingHistoryResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("record/delete"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeleteMeetingRecordResponse))]
    public async Task<IActionResult> DeleteMeetingRecordAsync([FromBody] DeleteMeetingRecordCommand command)
    {
        var response = await _mediator.SendAsync<DeleteMeetingRecordCommand, DeleteMeetingRecordResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("recording/start"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StartMeetingRecordingResponse))]
    public async Task<IActionResult> StartMeetingRecordingAsync([FromBody] StartMeetingRecordingCommand command)
    {
        var response = await _mediator.SendAsync<StartMeetingRecordingCommand, StartMeetingRecordingResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("invite"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StartMeetingRecordingResponse))]
    public async Task<IActionResult> MeetingInviteAsync([FromQuery] MeetingInviteRequest command)
    {
        var response = await _mediator.RequestAsync<MeetingInviteRequest, MeetingInviteResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
}