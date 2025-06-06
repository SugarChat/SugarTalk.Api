using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages.Dto.Meetings;
using Microsoft.AspNetCore.Authorization;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Commands.Meetings.Speak;
using SugarTalk.Messages.Commands.Meetings.Summary;

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
    
    [Route("switch/ea"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MeetingSwitchEaResponse))]
    public async Task<IActionResult> MeetingSwitchEaAsync([FromBody] MeetingSwitchEaCommand command)
    {
        var response = await _mediator.SendAsync<MeetingSwitchEaCommand, MeetingSwitchEaResponse>(command).ConfigureAwait(false);

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
    
    [Route("lock"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SetMeetingLockStatusResponse))]
    public async Task<IActionResult> SetMeetingLockStatusAsync(SetMeetingLockStatusCommand command)
    {
        var response = await _mediator.SendAsync<SetMeetingLockStatusCommand, SetMeetingLockStatusResponse>(command).ConfigureAwait(false);
        
        return Ok(response);
    }

    #region Appointment Meeting

    [Route("appointment"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAppointmentMeetingsResponse))]
    public async Task<IActionResult> GetAppointmentMeetingsAsync([FromQuery] GetAppointmentMeetingsRequest request)
    {
        var response = await _mediator.RequestAsync<GetAppointmentMeetingsRequest, GetAppointmentMeetingsResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("appointment/cancel"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CancelAppointmentMeetingResponse))]
    public async Task<IActionResult> CancelAppointmentMeetingAsync([FromBody] CancelAppointmentMeetingCommand command)
    {
        var response = await _mediator.SendAsync<CancelAppointmentMeetingCommand, CancelAppointmentMeetingResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [Route("appointment/detail"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAppointmentMeetingDetailResponse))]
    public async Task<IActionResult> GetAppointmentMeetingDetailAsync([FromQuery] GetAppointmentMeetingDetailRequest request)
    {
        var response = await _mediator.RequestAsync<GetAppointmentMeetingDetailRequest, GetAppointmentMeetingDetailResponse>(request);
        
        return Ok(response);
    }

    [Route("Get/staffs"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetStaffsTreeResponse))]
    public async Task<IActionResult> GetStaffsTreeAsync([FromQuery] GetStaffsTreeRequest request)
    {
        var response = await _mediator.RequestAsync<GetStaffsTreeRequest, GetStaffsTreeResponse>(request);
        
        return Ok(response);
    }

    #endregion

    #region MeetingHistory

    [Route("history"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingHistoriesByUserResponse))]
    public async Task<IActionResult> GetMeetingHistoriesByUserAsync([FromQuery] GetMeetingHistoriesByUserRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingHistoriesByUserRequest, GetMeetingHistoriesByUserResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("history/delete"), HttpPost]
    public async Task<IActionResult> DeleteMeetingHistoryAsync([FromBody] DeleteMeetingHistoryCommand command)
    {
        await _mediator.SendAsync(command).ConfigureAwait(false);

        return Ok();
    }

    #endregion

    #region Speak Detail

    [Route("record/speak"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RecordMeetingSpeakResponse))]
    public async Task<IActionResult> RecordMeetingSpeakAsync([FromBody] RecordMeetingSpeakCommand command)
    {
        var response = await _mediator.SendAsync<RecordMeetingSpeakCommand, RecordMeetingSpeakResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    #endregion

    #region Summary

    [Route("summary"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SummaryMeetingRecordResponse))]
    public async Task<IActionResult> SummaryMeetingRecordAsync([FromBody] SummaryMeetingRecordCommand command)
    {
        var response = await _mediator.SendAsync<SummaryMeetingRecordCommand, SummaryMeetingRecordResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
    
    #endregion

    #region MeetingRecord

    [Route("record"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetCurrentUserMeetingRecordResponse))]
    public async Task<IActionResult> GetCurrentUserMeetingRecordAsync([FromQuery] GetCurrentUserMeetingRecordRequest request)
    {
        var response = await _mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("record/detail"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingRecordDetailsResponse))]
    public async Task<IActionResult> GetMeetingRecordDetailsAsync([FromQuery] GetMeetingRecordDetailsRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingRecordDetailsRequest, GetMeetingRecordDetailsResponse>(request).ConfigureAwait(false);

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
    
    [Route("recording/stop"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StorageMeetingRecordVideoResponse))]
    public async Task<IActionResult> StorageMeetingRecordVideoAsync(StorageMeetingRecordVideoCommand command)
    {
        var response = await _mediator.SendAsync<StorageMeetingRecordVideoCommand, StorageMeetingRecordVideoResponse>(command).ConfigureAwait(false);
        
        return Ok(response);
    }

    [Route("recording/translation"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TranslatingMeetingSpeakResponse))]
    public async Task<IActionResult> TranslatingMeetingSpeakAsync([FromBody] TranslatingMeetingSpeakCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<TranslatingMeetingSpeakCommand, TranslatingMeetingSpeakResponse>(command, cancellationToken).ConfigureAwait(false);

        return Ok(response);
    }

    [Route("recording/url"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenerateMeetingRecordTemporaryUrlResponse))]
    public async Task<IActionResult> GenerateMeetingRecordTemporaryUrlAsync([FromBody] GenerateMeetingRecordTemporaryUrlCommand command)
    {
        var response = await _mediator.SendAsync<GenerateMeetingRecordTemporaryUrlCommand, GenerateMeetingRecordTemporaryUrlResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("record/update"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateMeetingRecordUrlResponse))]
    public async Task<IActionResult> UpdateMeetingRecordUrlAsync([FromBody] UpdateMeetingRecordUrlCommand command)
    {
        var response = await _mediator.SendAsync<UpdateMeetingRecordUrlCommand, UpdateMeetingRecordUrlResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("record/count"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingRecordCountResponse))]
    public async Task<IActionResult> GetMeetingRecordCountAsync([FromQuery] GetMeetingRecordCountRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingRecordCountRequest, GetMeetingRecordCountResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }
    
    #endregion
    
    #region MeetingUserSession

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
    
    [Route("get/user"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingOnlineLongestDurationUserResponse))]
    public async Task<IActionResult> GetMeetingUserSessionByMeetingIdAsync([FromQuery] Guid meetingId)
    {
        var response =
            await _mediator.RequestAsync<GetMeetingOnlineLongestDurationUserRequest, GetMeetingOnlineLongestDurationUserResponse>(
                new GetMeetingOnlineLongestDurationUserRequest { MeetingId = meetingId}).ConfigureAwait(false);
        
        return Ok(response);
    }

    [Route("update/role"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateMeetingUserSessionRoleResponse))]
    public async Task<IActionResult> UpdateMeetingUserSessionRoleAsync([FromBody] UpdateMeetingUserSessionRoleCommand command)
    {
        var response = await _mediator.SendAsync<UpdateMeetingUserSessionRoleCommand, UpdateMeetingUserSessionRoleResponse>(command).ConfigureAwait(false);
        
        return Ok(response);
    }

    [Route("get/all"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAllMeetingUserSessionsForMeetingIdResponse))]
    public async Task<IActionResult> GetAllMeetingUserSessionsForMeetingIdAsync([FromQuery] GetAllMeetingUserSessionsForMeetingIdRequest request)
    {
        var response = await _mediator.RequestAsync<GetAllMeetingUserSessionsForMeetingIdRequest, GetAllMeetingUserSessionsForMeetingIdResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }

    [Route("check/rename"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckRenamePermissionResponse))]
    public async Task<IActionResult> CheckRenamePermissionAsync(CheckRenamePermissionCommand command)
    {
        var response = await _mediator.SendAsync<CheckRenamePermissionCommand, CheckRenamePermissionResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    #endregion
    
    #region invite

    [Route("invite/{meetingNumber}"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JoinMeetingResponse))]
    public async Task<IActionResult> MeetingInviteAsync(string meetingNumber, [FromBody] MeetingInviteRequestDto requestData)
    {
        var response = await _mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(new JoinMeetingCommand
        {
            MeetingNumber = meetingNumber,
            SecurityCode = requestData.SecurityCode
        }).ConfigureAwait(false);

        return Ok(response);
    }
    
    [Route("invite/info"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingInviteInfoResponse))]
    public async Task<IActionResult> GetMeetingInviteInfoAsync([FromQuery] GetMeetingInviteInfoRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingInviteInfoRequest, GetMeetingInviteInfoResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }

    #endregion

    #region FeedBack

    [Route("feedback"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingProblemFeedbackResponse))]
    public async Task<IActionResult> GetMeetingProblemFeedBacksAsync([FromQuery] GetMeetingProblemFeedbackRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingProblemFeedbackRequest, GetMeetingProblemFeedbackResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }
    
    [Route("feedback/add"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddMeetingProblemFeedbackResponse))]
    public async Task<IActionResult> AddMeetingProblemFeedBacksAsync([FromBody] AddMeetingProblemFeedbackCommand command)
    {
        var response = await _mediator.SendAsync<AddMeetingProblemFeedbackCommand, AddMeetingProblemFeedbackResponse>(command).ConfigureAwait(false);
        
        return Ok(response);
    }
    
    [Route("feedback/count/get"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingProblemFeedbacksCountResponse))]
    public async Task<IActionResult> GetMeetingProblemFeedbacksCountAsync([FromQuery] GetMeetingProblemFeedbacksCountRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingProblemFeedbacksCountRequest, GetMeetingProblemFeedbacksCountResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }
    
    [Route("feedback/update"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateMeetingProblemFeedbackUnreadCountResponse))]
    public async Task<IActionResult> UpdateMeetingProblemFeedbackUnreadCountAsync([FromBody] UpdateMeetingProblemFeedbackUnreadCountCommand command)
    {
        var response = await _mediator.SendAsync<UpdateMeetingProblemFeedbackUnreadCountCommand, UpdateMeetingProblemFeedbackUnreadCountResponse>(command).ConfigureAwait(false);
        
        return Ok(response);
    }

    #endregion

    #region MeetingData

    [Route("data"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingDataResponse))]
    public async Task<IActionResult> GetMeetingDataAsync([FromQuery] GetMeetingDataRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingDataRequest, GetMeetingDataResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }
    
    [Route("data/user"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMeetingDataUserResponse))]
    public async Task<IActionResult> GetMeetingDataUserAsync([FromQuery] GetMeetingDataUserRequest request)
    {
        var response = await _mediator.RequestAsync<GetMeetingDataUserRequest, GetMeetingDataUserResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }

    #endregion
}           