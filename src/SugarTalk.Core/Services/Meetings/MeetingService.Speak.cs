using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Domain.Meeting;
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
        MeetingSpeakDetail speakDetail = null;
        
        if (command is { Id: null, SpeakStartTime: not null })
        {
            speakDetail = new MeetingSpeakDetail
            {
                MeetingId = command.MeetingId,
                UserId = _currentUser.Id.Value,
                MeetingSubId = command.MeetingSubId,
                SpeakStartTime = command.SpeakStartTime.Value
            };
            
            await _meetingDataProvider.AddMeetingSpeakDetailAsync(speakDetail, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        
        if (command is { Id: not null, SpeakEndTime: not null })
        {
            speakDetail = (await _meetingDataProvider.GetMeetingSpeakDetailsAsync(
                command.Id.Value, userId: _currentUser.Id.Value, speakStatus: SpeakStatus.Speaking, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();

            if (speakDetail == null) throw new SpeakNotFoundException();

            speakDetail.SpeakStatus = SpeakStatus.End;
            speakDetail.SpeakEndTime = command.SpeakEndTime.Value;
            
            await _meetingDataProvider.UpdateMeetingSpeakDetailAsync(speakDetail, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        if (speakDetail != null)
            speakDetail = (await _meetingDataProvider.GetMeetingSpeakDetailsAsync(
                speakDetail.Id, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        
        return new MeetingSpeakRecordedEvent
        {
            MeetingSpeakDetail = _mapper.Map<MeetingSpeakDetailDto>(speakDetail)
        };
    }
}