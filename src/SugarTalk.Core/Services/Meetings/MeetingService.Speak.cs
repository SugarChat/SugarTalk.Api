using System;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Enums.Meeting.Speak;
using SugarTalk.Messages.Events.Meeting.Speak;
using SugarTalk.Messages.Commands.Meetings.Speak;
using SugarTalk.Core.Services.Meetings.Exceptions;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<MeetingSpeakRecordedEvent> RecordMeetingSpeakAsync(RecordMeetingSpeakCommand command, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<MeetingSpeakRecordedEvent> RecordMeetingSpeakAsync(RecordMeetingSpeakCommand command, CancellationToken cancellationToken)
    {
        var speakDetail = command.Id.HasValue switch
        {
            true => await EndRecordUserSpeakDetailAsync(command, cancellationToken).ConfigureAwait(false),
            false => await StartRecordUserSpeakDetailAsync(command, cancellationToken).ConfigureAwait(false)
        };

        var result = (await _meetingDataProvider.GetMeetingSpeakDetailsAsync(
            speakDetail.Id, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        
        return new MeetingSpeakRecordedEvent
        {
            MeetingSpeakDetail = _mapper.Map<MeetingSpeakDetailDto>(result)
        };
    }
    
    private async Task<MeetingSpeakDetail> StartRecordUserSpeakDetailAsync(RecordMeetingSpeakCommand command, CancellationToken cancellationToken)
    {
        //todo get record token
        var token = string.Empty;
        var filePath = string.Empty;
        
        var egressResponse = await _liveKitClient.StartTrackCompositeEgressAsync(new StartTrackCompositeEgressRequestDto
        {
            Token = token,
            RoomName = command.RoomNumber,
            AudioTrackId = command.TrackId,
            Files = new EgressEncodedFileOutPutDto
            {
                Filepath = filePath,
                AliOssUpload = new EgressAliOssUploadDto()
            }
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("Start track composite egress response: {@EgressResponse}", egressResponse);

        if (egressResponse == null) throw new Exception();
        
        var speakDetail = new MeetingSpeakDetail
        {
            FilePath = filePath,
            TrackId = command.TrackId,
            UserId = _currentUser.Id.Value,
            RoomNumber = command.RoomNumber,
            EgressId = egressResponse.EgressId,
            MeetingRecordId = command.MeetingRecordId,
            SpeakStartTime = command.SpeakStartTime.Value
        };
        
        await _meetingDataProvider.AddMeetingSpeakDetailAsync(speakDetail, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        return speakDetail;
    }
    
    private async Task<MeetingSpeakDetail> EndRecordUserSpeakDetailAsync(RecordMeetingSpeakCommand command, CancellationToken cancellationToken)
    {
        var speakDetail = (await _meetingDataProvider.GetMeetingSpeakDetailsAsync(
            command.Id.Value, speakStatus: SpeakStatus.Speaking, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        Log.Information("Ending record user Speak, speak detail: {@SpeakDetail}", speakDetail);
        
        if (speakDetail == null) throw new SpeakNotFoundException();
        
        var egressResponse = await _liveKitClient.StopEgressAsync(new StopEgressRequestDto
        {
            EgressId = speakDetail.EgressId
        }, cancellationToken).ConfigureAwait(false);
        
        Log.Information("Stop egress response: {@EgressResponse}", egressResponse);

        if (string.IsNullOrEmpty(egressResponse)) throw new Exception();
        
        speakDetail.SpeakStatus = SpeakStatus.End;
        speakDetail.SpeakEndTime = command.SpeakEndTime.Value;
        
        await _meetingDataProvider.UpdateMeetingSpeakDetailAsync(speakDetail, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        return speakDetail;
    }
}