using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Domain.Meeting;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task AddMeetingInvitationRecordsAsync(List<MeetingInvitationRecord> records, bool forceSave = true, CancellationToken cancellationToken = default);
}

public partial class MeetingDataProvider
{
    public async Task AddMeetingInvitationRecordsAsync(List<MeetingInvitationRecord> records, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAllAsync(records, cancellationToken).ConfigureAwait(false);

        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}