using System;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<GetCurrentUserMeetingRecordResponse> GetCurrentUserMeetingRecordsAsync(GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken);
    
    Task DeleteMeetingRecordAsync(DeleteMeetingRecordCommand command, CancellationToken cancellationToken);

    Task<MeetingRecordingStartedEvent> StartMeetingRecordingAsync(StartMeetingRecordingCommand command, CancellationToken cancellationToken);
    
    Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(GetMeetingRecordDetailsRequest request, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<GetCurrentUserMeetingRecordResponse> GetCurrentUserMeetingRecordsAsync(GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken)
    {
        var (total, items) = await _meetingDataProvider.GetMeetingRecordsByUserIdAsync(_currentUser.Id, request, cancellationToken).ConfigureAwait(false);

        var response = new GetCurrentUserMeetingRecordResponse
        {
           Data = new GetCurrentUserMeetingRecordResponseDto
           {
               Count = total,
               Records = items
           }
        };

        return response;
    }

    public async Task<MeetingRecordingStartedEvent> StartMeetingRecordingAsync(StartMeetingRecordingCommand command, CancellationToken cancellationToken)
    {
        var meetingRecordId = Guid.NewGuid();
        
        var meeting = await _meetingDataProvider
            .GetMeetingByIdAsync(command.MeetingId, cancellationToken).ConfigureAwait(false);

        if (meeting is null) throw new MeetingNotFoundException();

        if (meeting.MeetingMasterUserId != _currentUser?.Id) 
            throw new CannotStartMeetingRecordingException(_currentUser?.Id);

        var user = await _accountDataProvider.GetUserAccountAsync(_currentUser.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

        var postResponse = await _liveKitClient.StartTrackCompositeEgressAsync(new StartTrackCompositeEgressRequestDto
        {
            Token = _liveKitServerUtilService.GenerateTokenForRecordMeeting(user, meeting.MeetingNumber),
            RoomName = meeting.MeetingNumber,
            File = new EgressEncodedFileOutPutDto
            {
                FilePath = $"SugarTalk/{meetingRecordId}.mp4",
                AliOssUpload = new EgressAliOssUploadDto
                {
                    AccessKey = _aliYunOssSetting.AccessKeyId,
                    Secret = _aliYunOssSetting.AccessKeySecret,
                    Bucket = _aliYunOssSetting.BucketName,
                    Endpoint = _aliYunOssSetting.Endpoint
                }
            }
        }, cancellationToken).ConfigureAwait(false);

        if (postResponse is null) throw new Exception("Start Meeting Recording Failed.");

        await _meetingDataProvider.PersistMeetingRecordAsync(meeting.Id, meetingRecordId, cancellationToken).ConfigureAwait(false);

        return new MeetingRecordingStartedEvent
        {
            MeetingRecordId = meetingRecordId,
            EgressId = postResponse.EgressId
        };
    }
    
    public async Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(GetMeetingRecordDetailsRequest request, CancellationToken cancellationToken)
    {
        var response =  await _meetingDataProvider.GetMeetingRecordDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
        
        return new GetMeetingRecordDetailsResponse
        {
            Data = response
        };
    }
}