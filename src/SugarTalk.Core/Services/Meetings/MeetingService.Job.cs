using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial class MeetingService
{
    public async Task ScheduleAutoDeactivateMeetingAsync(
        ScheduleAutoDeactivateMeetingCommand command, CancellationToken cancellationToken)
    {
        var meetings = await _meetingDataProvider.GetMeetingsAsync(cancellationToken).ConfigureAwait(false);

        meetings = meetings.Where(x => x.EndDate <= _clock.Now.ToUnixTimeSeconds()).ToList();

        await _meetingDataProvider.RemoveMeetingsAsync(meetings, cancellationToken).ConfigureAwait(false);
    }
}