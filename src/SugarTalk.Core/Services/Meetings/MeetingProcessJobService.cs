using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Data;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Services.Meetings;

public interface IMeetingProcessJobService : IScopedDependency
{
    Task UpdateRepeatMeetingAsync(UpdateRepeatMeetingCommand command, CancellationToken cancellationToken);
    
    Task CheckAppointmentMeetingDateAsync(CheckAppointmentMeetingDateCommand command, CancellationToken cancellationToken);
}

public class MeetingProcessJobService : IMeetingProcessJobService
{
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMeetingDataProvider _meetingDataProvider;

    public MeetingProcessJobService(IClock clock, IUnitOfWork unitOfWork, IMeetingDataProvider meetingDataProvider)
    {
        _clock = clock;
        _unitOfWork = unitOfWork;
        _meetingDataProvider = meetingDataProvider;
    }

    public async Task UpdateRepeatMeetingAsync(UpdateRepeatMeetingCommand command, CancellationToken cancellationToken)
    {
        var repeatMeetings =
            await _meetingDataProvider.GetAvailableRepeatMeetingAsync(cancellationToken).ConfigureAwait(false);

        if (repeatMeetings is not { Count: > 0 }) return;

        var meetingIds = repeatMeetings.Select(x => x.Id);

        var subMeetings = 
            await _meetingDataProvider.GetMeetingSubMeetingsAsync(meetingIds, cancellationToken).ConfigureAwait(false);

        var subMeetingGroupByMeetingIds = subMeetings.GroupBy(x => x.MeetingId).ToList();

        foreach (var group in subMeetingGroupByMeetingIds)
        {
            var earliestSubMeeting = group.MinBy(x => x.StartTime);

            if (earliestSubMeeting is null) continue;

            var updatedMeeting = repeatMeetings.FirstOrDefault(x => x.Id == group.Key);

            if (updatedMeeting is null) continue;
            
            updatedMeeting.StartDate = earliestSubMeeting.StartTime;
            updatedMeeting.EndDate = earliestSubMeeting.EndTime;
            updatedMeeting.Status = MeetingStatus.Pending;
            
            await _meetingDataProvider.UpdateMeetingAsync(updatedMeeting, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task CheckAppointmentMeetingDateAsync(CheckAppointmentMeetingDateCommand command, CancellationToken cancellationToken)
    {
        var appointmentMeetings = await _meetingDataProvider
            .GetAllAppointmentMeetingWithPendingAndInProgressAsync(cancellationToken).ConfigureAwait(false);

        foreach (var appointmentMeeting in appointmentMeetings)
        {
            if (appointmentMeeting.StartDate < _clock.Now.ToUnixTimeSeconds() && appointmentMeeting.EndDate > _clock.Now.ToUnixTimeSeconds())
            {
                appointmentMeeting.Status = MeetingStatus.InProgress;
            }
            
            if(appointmentMeeting.EndDate <= _clock.Now.ToUnixTimeSeconds())
            {
                appointmentMeeting.Status = MeetingStatus.Pending;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}