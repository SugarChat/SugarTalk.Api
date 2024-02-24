using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Data;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public interface IMeetingProcessJobService : IScopedDependency
{
    Task UpdateRepeatMeetingAsync(UpdateRepeatMeetingCommand command, CancellationToken cancellationToken);
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
        var allAppointmentMeeting = await _meetingDataProvider.GetAllRepeatMeetingAsync(cancellationToken).ConfigureAwait(false);

        if (allAppointmentMeeting is not { Count: > 0 }) return;

        var meetingIds = allAppointmentMeeting.Select(x => x.Id);

        var subMeetings = await _meetingDataProvider.GetMeetingSubMeetingsAsync(meetingIds, cancellationToken).ConfigureAwait(false);
    }
}