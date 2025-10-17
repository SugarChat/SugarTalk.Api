using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task AddMeetingInvitationRecordsAsync(List<MeetingInvitationRecord> records, bool forceSave = true, CancellationToken cancellationToken = default);

    Task<List<GetMeetingInvitationRecordsDto>> GetMeetingInvitationRecordsDtoAsync(int userId, CancellationToken cancellationToken);

    Task<List<MeetingInvitationRecord>> GetMeetingInvitationRecordsAsync(List<int> ids, CancellationToken cancellationToken);

    Task UpdateMeetingInvitationRecordsAsync(List<MeetingInvitationRecord> records, bool forceSave = true, CancellationToken cancellationToken = default);
}

public partial class MeetingDataProvider
{
    public async Task AddMeetingInvitationRecordsAsync(List<MeetingInvitationRecord> records, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAllAsync(records, cancellationToken).ConfigureAwait(false);

        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<GetMeetingInvitationRecordsDto>> GetMeetingInvitationRecordsDtoAsync(int userId, CancellationToken cancellationToken)
    {
        var originalQuery = _repository.QueryNoTracking<MeetingInvitationRecord>(x => x.BeInviterUserId == userId);

        return await (from invitationRecord in originalQuery
            join meeting in _repository.QueryNoTracking<Meeting>() on invitationRecord.MeetingId equals meeting.Id
            orderby meeting.CreatedDate
            select new GetMeetingInvitationRecordsDto
            {
                Id = invitationRecord.Id,
                InvitingPeople = invitationRecord.UserName,
                MeetingTitle = meeting.Title
            }).ToListAsync(cancellationToken);
    }

    public async Task<List<MeetingInvitationRecord>> GetMeetingInvitationRecordsAsync(List<int> ids, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingInvitationRecord>().Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateMeetingInvitationRecordsAsync(List<MeetingInvitationRecord> records, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAllAsync(records, cancellationToken).ConfigureAwait(false);

        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}