<<<<<<< HEAD
﻿using System;
=======
using System;
>>>>>>> main
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task UpdateMeetingRecordAsync(MeetingRecord record, CancellationToken cancellationToken);
    
    Task<MeetingRecord> GetMeetingRecordByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken);
   
    public Task<(int count, List<MeetingRecordDto> items)> GetMeetingRecordsByUserIdAsync(int? currentUserId, GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken);
}

public partial class MeetingDataProvider
{
    public async Task<(int count, List<MeetingRecordDto> items)> GetMeetingRecordsByUserIdAsync(int? currentUserId, GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken)
    {
        if (currentUserId == null)
        {
            return (0, new List<MeetingRecordDto>());
        }
        var query = _repository.QueryNoTracking<MeetingRecord>()
            .Join(_repository.QueryNoTracking<Meeting>(), record => record.MeetingId, meeting => meeting.Id,
                (record, meeting) => new
                {
                    Record = record,
                    Meeting = meeting
                })
            .Join(_repository.QueryNoTracking<MeetingUserSession>(), result => result.Meeting.Id,
                session => session.MeetingId,
                (result, session) => new
                {
                    result.Meeting,
                    result.Record,
                    Session = session
                })
            .Join(_repository.QueryNoTracking<UserAccount>(), result => result.Meeting.MeetingMasterUserId,
                user => user.Id,
                (result, user) => new
                {
                    result.Meeting,
                    result.Record,
                    result.Session,
                    User = user
                })
            .Where(x => x.Session.UserId == currentUserId);

        query = string.IsNullOrEmpty(request.Keyword) ? query : query.Where(x =>
                x.Meeting.Title.Contains(request.Keyword) ||
                x.Meeting.MeetingNumber.Contains(request.Keyword) ||
                x.User.UserName.Contains(request.Keyword));

        query = string.IsNullOrEmpty(request.MeetingTitle) ? query : query.Where(x => x.Meeting.Title.Contains(request.MeetingTitle));
        query = string.IsNullOrEmpty(request.MeetingNumber) ? query : query.Where(x => x.Meeting.MeetingNumber.Contains(request.MeetingNumber));
        query = string.IsNullOrEmpty(request.Creator) ? query : query.Where(x => x.User.UserName.Contains(request.Creator));

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var joinResult = await query
            .OrderByDescending(x => x.Record.CreatedDate)
            .Skip((request.PageSetting.Page - 1) * request.PageSetting.PageSize)
            .Take(request.PageSetting.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = joinResult.Select(x => new MeetingRecordDto
            {
                MeetingId = x.Meeting.Id,
                MeetingNumber = x.Meeting.MeetingNumber,
                RecordNumber = x.Record.RecordNumber,
                Title = x.Meeting.Title,
                StartDate = x.Meeting.StartDate,
                EndDate = x.Meeting.EndDate,
                Timezone = x.Meeting.TimeZone,
                MeetingCreator = x.User.UserName,
                Duration = CalculateMeetingDuration(x.Meeting.StartDate, x.Meeting.EndDate)
            })
            .ToList();

        return (total, items);
    }
    
    public async Task UpdateMeetingRecordAsync(MeetingRecord record,CancellationToken cancellationToken)
    {
        if (record == null) return;
        
        await _repository.UpdateAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MeetingRecord> GetMeetingRecordByMeetingIdAsync(Guid meetingId,CancellationToken cancellationToken)
    {
        var meetingRecord = await _repository.QueryNoTracking<MeetingRecord>(x => x.MeetingId == meetingId)
            .OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return meetingRecord;
    }
}