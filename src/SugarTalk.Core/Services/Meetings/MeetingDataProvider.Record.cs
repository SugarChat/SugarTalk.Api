using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<List<MeetingRecord>> GetMeetingRecordsAsync(Guid? id = null, CancellationToken cancellationToken = default);
    
    Task<(int count, List<MeetingRecordDto> items)> GetMeetingRecordsByUserIdAsync(int? currentUserId, GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken);
    
    Task DeleteMeetingRecordAsync(List<Guid> meetingRecordIds, CancellationToken cancellationToken);
    
    Task PersistMeetingRecordAsync(Guid meetingId, Guid meetingRecordId, string egressId, CancellationToken cancellationToken);
    
    Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(Guid recordId, CancellationToken cancellationToken);
    
    Task UpdateMeetingRecordAsync(MeetingRecord record, CancellationToken cancellationToken);
    
    Task<MeetingRecord> GetNewestMeetingRecordByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken);

    Task<MeetingRecord> GetMeetingRecordByMeetingRecordIdAsync(Guid meetingRecordId, CancellationToken cancellationToken);

    Task<MeetingRecord> GetMeetingRecordByRecordIdAsync(Guid meetingRecordId, CancellationToken cancellationToken);
    
    Task UpdateMeetingRecordUrlStatusAsync(Guid meetingRecordId, MeetingRecordUrlStatus urlStatus, CancellationToken cancellationToken);
    
    Task<List<MeetingSpeakDetail>> GetMeetingDetailsByRecordIdAsync(Guid meetingRecordId, CancellationToken cancellationToken);
}

public partial class MeetingDataProvider
{
    public async Task<List<MeetingRecord>> GetMeetingRecordsAsync(
        Guid? id = null, CancellationToken cancellationToken = default)
    {
        var query = _repository.Query<MeetingRecord>();

        if (id.HasValue)
            query = query.Where(x => x.Id == id.Value);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<(int count, List<MeetingRecordDto> items)> GetMeetingRecordsByUserIdAsync(int? currentUserId, GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken)
    {
        if (currentUserId == null)
        {
            return (0, new List<MeetingRecordDto>());
        }

        var query = _repository.QueryNoTracking<MeetingRecord>()
            .Join(_repository.QueryNoTracking<Meeting>(), 
                record => record.MeetingId, 
                meeting => meeting.Id,
                (Record, Meeting) => new { Record, Meeting })
            .Join(_repository.QueryNoTracking<MeetingUserSession>(), 
                rm => rm.Meeting.Id,
                session => session.MeetingId,
                (rm, session) => new { rm.Record, rm.Meeting, session })
            .Join(_repository.QueryNoTracking<UserAccount>(), 
                rms => rms.Meeting.MeetingMasterUserId,
                user => user.Id,
                (rms, User) => new { rms.Record, rms.Meeting, rms.session, User })
            .Where(x => x.session.UserId == currentUserId && !x.Record.IsDeleted);

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
            .Select(x => new MeetingRecordDto
            {
                MeetingRecordId = x.Record.Id,
                MeetingId = x.Meeting.Id,
                MeetingNumber = x.Meeting.MeetingNumber,
                RecordNumber = x.Record.RecordNumber,
                Title = x.Meeting.Title,
                StartDate = x.Meeting.StartDate,
                EndDate = x.Meeting.EndDate,
                Timezone = x.Meeting.TimeZone,
                MeetingCreator = x.User.UserName,
                Duration = CalculateMeetingDuration(x.Meeting.StartDate, x.Meeting.EndDate),
                Url = x.Record.Url,
                UrlStatus = x.Record.UrlStatus
            }).ToListAsync(cancellationToken);

        var items = joinResult.GroupBy(x => x.MeetingRecordId).Select(g => g.First()).ToList();

        return (total, items);
    }

    public async Task DeleteMeetingRecordAsync(List<Guid> meetingRecordIds, CancellationToken cancellationToken)
    {
        var meetingRecords = await _repository.Query<MeetingRecord>()
            .Where(x => meetingRecordIds.Contains(x.Id))
            .Where(x => !x.IsDeleted).ToListAsync(cancellationToken).ConfigureAwait(false);

        if (meetingRecords is not { Count: > 0 }) return;
        
        meetingRecords.ForEach(x => x.IsDeleted = true);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task PersistMeetingRecordAsync(Guid meetingId, Guid meetingRecordId, string egressId, CancellationToken cancellationToken)
    {
        var meetingRecordTotal = await _repository.Query<MeetingRecord>()
            .CountAsync(x => x.MeetingId == meetingId, cancellationToken).ConfigureAwait(false);

        await _repository.InsertAsync(new MeetingRecord
        {
            Id = meetingRecordId,
            MeetingId = meetingId,
            EgressId = egressId,
            RecordNumber = GenerateRecordNumber(++meetingRecordTotal)
        }, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(Guid recordId, CancellationToken cancellationToken)
    {
        var currentUser = await _accountDataProvider
            .GetUserAccountAsync(_currentUser.Id.Value, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (currentUser is null) throw new UnauthorizedAccessException();
        
        var meetingRecordDetails = await _repository.QueryNoTracking<MeetingSpeakDetail>()
            .Where(x => x.MeetingRecordId == recordId).ToListAsync(cancellationToken).ConfigureAwait(false);

        var meetingInfo = await (
            from meetingRecord in _repository.QueryNoTracking<MeetingRecord>()
            join meeting in _repository.QueryNoTracking<Meeting>() on meetingRecord.MeetingId equals meeting.Id
            join meetingSummary in _repository.QueryNoTracking<MeetingSummary>() on meetingRecord.Id equals meetingSummary.RecordId
            into meetingSummaryLeft
            from meetingSummary in  meetingSummaryLeft.DefaultIfEmpty()
            where meetingRecord.Id == recordId
            select new GetMeetingRecordDetailsDto
            {
                Id = recordId,
                MeetingTitle = meeting.Title,
                MeetingNumber = meeting.MeetingNumber,
                MeetingStartDate = meeting.StartDate,
                MeetingEndDate = meeting.EndDate,
                Url = meetingRecord.Url,
                Summary = meetingSummary == null? null : meetingSummary.Summary,
                MeetingRecordDetail = meetingRecordDetails.Select(x => _mapper.Map<MeetingSpeakDetailDto>(x)).ToList()
            }
        ).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return new GetMeetingRecordDetailsResponse
        {
            Data = meetingInfo
        };
    }

    private string GenerateRecordNumber(int total)
    {
        var sequenceToString = total.ToString().PadLeft(6, '0');

        return $"ZNZX-{_clock.Now.Year}{_clock.Now.Month}{_clock.Now.Day}{sequenceToString}";
    }

    public async Task UpdateMeetingRecordAsync(MeetingRecord record, CancellationToken cancellationToken)
    {
        if (record == null) return;

        await _repository.UpdateAsync(record, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<MeetingRecord> GetNewestMeetingRecordByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken)
    {
        return await _repository
            .QueryNoTracking<MeetingRecord>(x => x.MeetingId == meetingId && x.RecordType == MeetingRecordType.OnRecord)
            .OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<MeetingRecord> GetMeetingRecordByMeetingRecordIdAsync(Guid meetingRecordId, CancellationToken cancellationToken)
    {
        return await _repository
            .Query<MeetingRecord>(x => x.Id == meetingRecordId && x.RecordType == MeetingRecordType.OnRecord)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<MeetingRecord> GetMeetingRecordByRecordIdAsync(Guid meetingRecordId, CancellationToken cancellationToken)
    {
        return await _repository.QueryNoTracking<MeetingRecord>(x => x.Id == meetingRecordId)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateMeetingRecordUrlStatusAsync(Guid meetingRecordId, MeetingRecordUrlStatus urlStatus, CancellationToken cancellationToken)
    {
        var meetingRecord = await _repository.Query<MeetingRecord>()
            .FirstOrDefaultAsync(x => x.Id == meetingRecordId, cancellationToken).ConfigureAwait(false);

        if (meetingRecord == null) return;

        meetingRecord.UrlStatus = urlStatus;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingSpeakDetail>> GetMeetingDetailsByRecordIdAsync(Guid meetingRecordId, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingSpeakDetail>().Where(x=>x.MeetingRecordId == meetingRecordId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}