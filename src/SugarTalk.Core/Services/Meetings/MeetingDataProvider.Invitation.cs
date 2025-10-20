using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task AddMeetingInvitationRecordsAsync(List<MeetingInvitationRecord> records, bool forceSave = true, CancellationToken cancellationToken = default);

    Task<List<GetMeetingInvitationRecordsDto>> GetMeetingInvitationRecordsDtoAsync(int userId, CancellationToken cancellationToken);

    Task<List<MeetingInvitationRecord>> GetMeetingInvitationRecordsAsync(List<int> ids = null, Guid? meetingId = null, Guid? meetingSubId = null, CancellationToken cancellationToken = default);

    Task UpdateMeetingInvitationRecordsAsync(List<MeetingInvitationRecord> records, bool forceSave = true, CancellationToken cancellationToken = default);

    Task<List<NoJoinMeetingUserSessionsDto>> GetMeetingInvitationUserInfoAsync(Guid meetingId, Guid? meetingSubId, CancellationToken cancellationToken);
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
                MeetingTitle = meeting.Title,
                MeetingId = invitationRecord.MeetingId,
                MeetingSubId = invitationRecord.MeetingSubId 
            }).ToListAsync(cancellationToken);
    }

    public async Task<List<MeetingInvitationRecord>> GetMeetingInvitationRecordsAsync(List<int> ids = null, Guid? meetingId = null, Guid? meetingSubId = null, CancellationToken cancellationToken = default)
    {
        var query = _repository.Query<MeetingInvitationRecord>();

        if (ids is { Count: > 0 })
            query = query.Where(x => ids.Contains(x.Id));

        if (meetingId.HasValue)
            query = query.Where(x => x.MeetingId == meetingId);
        
        if (meetingSubId.HasValue)
            query = query.Where(x => x.MeetingSubId == meetingSubId);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateMeetingInvitationRecordsAsync(List<MeetingInvitationRecord> records, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAllAsync(records, cancellationToken).ConfigureAwait(false);

        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<NoJoinMeetingUserSessionsDto>> GetMeetingInvitationUserInfoAsync(Guid meetingId, Guid? meetingSubId, CancellationToken cancellationToken)
    {
        var query = _repository.QueryNoTracking<MeetingInvitationRecord>().Where(x => x.MeetingId == meetingId);

        if (meetingSubId.HasValue)
            query = query.Where(x => x.MeetingSubId == meetingSubId.Value);
        
        return await query.Join(_repository.Query<UserAccount>(), x => x.BeInviterUserId, s => s.Id, (record, account) => new NoJoinMeetingUserSessionsDto
        {
            Id = record.BeInviterUserId,
            UserName = account.UserName,
            InvitationStatus = record.InvitationStatus
        }).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}