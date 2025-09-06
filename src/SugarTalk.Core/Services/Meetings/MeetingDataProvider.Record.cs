using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SugarTalk.Messages.Dto.Translation;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Meetings.Speak;
using SugarTalk.Messages.Dto.Meetings.Summary;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<List<MeetingRecord>> GetMeetingRecordsAsync(Guid? id = null, Guid? meetingId = null, string egressId = null, CancellationToken cancellationToken = default);
    
    Task<(int count, List<MeetingRecordDto> items)> GetMeetingRecordsByUserIdAsync(int? currentUserId, GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken);
    
    Task DeleteMeetingRecordAsync(List<Guid> meetingRecordIds, CancellationToken cancellationToken);
    
    Task PersistMeetingRecordAsync(Guid meetingId, Guid meetingRecordId, string egressId, Guid? meetingSubId, CancellationToken cancellationToken);
    
    Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(Guid recordId, TranslationLanguage? language, CancellationToken cancellationToken);
    
    Task UpdateMeetingRecordAsync(MeetingRecord record, CancellationToken cancellationToken);
    
    Task<MeetingRecord> GetNewestMeetingRecordByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken);

    Task<MeetingRecord> GetMeetingRecordByMeetingRecordIdAsync(Guid meetingRecordId, CancellationToken cancellationToken);

    Task<MeetingRecord> GetMeetingRecordByRecordIdAsync(Guid meetingRecordId, CancellationToken cancellationToken);
    
    Task UpdateMeetingRecordUrlStatusAsync(Guid meetingRecordId, MeetingRecordUrlStatus urlStatus, CancellationToken cancellationToken);
    
    Task<List<MeetingSpeakDetail>> GetMeetingDetailsByRecordIdAsync(Guid meetingRecordId, CancellationToken cancellationToken);
    
    Task<List<MeetingSpeakDetailTranslationRecord>> GetMeetingDetailsTranslationRecordAsync(
        Guid meetingRecordId, TranslationLanguage language, int? meetingSpeakDetailId = null, CancellationToken cancellationToken = default);
    
    Task AddMeetingDetailsTranslationRecordAsync(List<MeetingSpeakDetailTranslationRecord> meetingSpeakDetails, CancellationToken cancellationToken);
    
    Task UpdateMeetingDetailTranslationRecordAsync(MeetingSpeakDetailTranslationRecord meetingSpeakDetail, CancellationToken cancellationToken);
    
    Task UpdateMeetingChatVoiceRecordAsync(MeetingChatVoiceRecord meetingChatVoiceRecord, bool forSave = true, CancellationToken cancellationToken = default);
    
    Task AddMeetingRecordVoiceRelayStationAsync(
        MeetingRestartRecord meetingRestartRecord, bool forSave = true, CancellationToken cancellationToken = default);
    
    Task<List<MeetingRestartRecord>> GetMeetingRecordVoiceRelayStationAsync(Guid meetingId, Guid recordId, CancellationToken cancellationToken);

    Task<MeetingSituationDay> GetMeetingSituationDayAsync(Guid meetingId, DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken);

    Task UpdateMeetingSituationDayAsync(MeetingSituationDay meetingSituationDay, bool foreSave = true, CancellationToken cancellationToken = default);

    Task AddMeetingSituationDayAsync(MeetingSituationDay meetingSituationDay, bool foreSave = true, CancellationToken cancellationToken = default);

    Task<List<GetMeetingDataDto>> GetMeetingSituationDaysAsync(DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken);

    Task<List<GetMeetingDataUserDto>> GetMeetingDataUserAsync(DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken);
    
    Task AddMeetingRestartRecordsAsync(List<MeetingRestartRecord> meetingRestartRecords, CancellationToken cancellationToken);
}

public partial class MeetingDataProvider
{
    public async Task<List<MeetingRecord>> GetMeetingRecordsAsync(
        Guid? id = null, Guid? meetingId = null, string egressId = null, CancellationToken cancellationToken = default)
    {
        var query = _repository.Query<MeetingRecord>();

        if (id.HasValue)
            query = query.Where(x => x.Id == id.Value);
        
        if (!string.IsNullOrEmpty(egressId))
            query = query.Where(x => x.EgressId == egressId);

        if (meetingId.HasValue)
            query = query.Where(x => x.MeetingId == meetingId.Value && x.RecordType == MeetingRecordType.OnRecord).OrderByDescending(x => x.CreatedDate);
        
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<(int count, List<MeetingRecordDto> items)> GetMeetingRecordsByUserIdAsync(int? currentUserId, GetCurrentUserMeetingRecordRequest request, CancellationToken cancellationToken)
    {
        if (currentUserId == null) return (0, new List<MeetingRecordDto>());

        var originalQuery = await _repository.QueryNoTracking<MeetingUserSession>()
            .Where(x => x.UserId == currentUserId)
            .Join(_repository.QueryNoTracking<MeetingRecord>().Where(x => !x.IsDeleted),
                session => new { session.MeetingId, session.MeetingSubId },
                record => new { record.MeetingId, record.MeetingSubId },
                (session, record) => new { session, record }).ToListAsync(cancellationToken).ConfigureAwait(false);

        var meetingIds = originalQuery.Select(x => x.session.MeetingId).ToList();

        var query = from meeting in _repository.QueryNoTracking<Meeting>().Where(x => meetingIds.Contains(x.Id))
            join user in _repository.QueryNoTracking<UserAccount>() on meeting.CreatedBy equals user.Id
            select new { meeting, user};

        if (!string.IsNullOrEmpty(request.Keyword))
            query = query.Where(x => x.meeting.Title.Contains(request.Keyword) || 
                                     x.meeting.MeetingNumber.Contains(request.Keyword) ||
                                     x.user.UserName.Contains(request.Keyword));

        if (!string.IsNullOrEmpty(request.MeetingTitle))
            query = query.Where(x => x.meeting.Title.Contains(request.MeetingTitle));
        
        if (!string.IsNullOrEmpty(request.MeetingNumber))
            query = query.Where(x => x.meeting.MeetingNumber.Contains(request.MeetingNumber));

        if (!string.IsNullOrEmpty(request.Creator))
            query = query.Where(x => x.user.UserName.Contains(request.Creator));

        var meetings = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        
        var targetQuery = meetings.Select(x =>
        {
            var record = originalQuery.FirstOrDefault(s => s.record.MeetingId == x.meeting.Id);
            if (record == null) return null;

            return new { x.meeting, x.user, record.record, record.session};
        }).ToList();
        
        var countQuery = targetQuery.GroupBy(x => x.record.Id).Select(g => g.First());
        
        var total = countQuery.Count();
        
        var joinResult = targetQuery
            .OrderByDescending(x => x.record.CreatedDate)
            .Skip((request.PageSetting.Page - 1) * request.PageSetting.PageSize)
            .Take(request.PageSetting.PageSize)
            .Select(x => new MeetingRecordDto
            {
                MeetingRecordId = x.record.Id,
                MeetingId = x.meeting.Id,
                MeetingNumber = x.meeting.MeetingNumber,
                RecordNumber = x.record.RecordNumber,
                Title = x.meeting.Title,
                StartDate = x.record.StartedAt.HasValue ? x.record.StartedAt.Value.ToUnixTimeSeconds() : 0,
                EndDate = x.record.EndedAt.HasValue ? x.record.EndedAt.Value.ToUnixTimeSeconds() : 0,
                Timezone = x.meeting.TimeZone,
                MeetingCreator = x.user.UserName,
                Duration = CalculateMeetingDuration(x.record.StartedAt.HasValue ? x.record.StartedAt.Value.ToUnixTimeSeconds() : 0, x.record.EndedAt.HasValue ? x.record.EndedAt.Value.ToUnixTimeSeconds() : 0),
                Url = x.record.Url,
                UrlStatus = x.record.UrlStatus
            }).ToList();
        
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

    public async Task PersistMeetingRecordAsync(Guid meetingId, Guid meetingRecordId, string egressId, Guid? meetingSubId, CancellationToken cancellationToken)
    {
        var meetingRecordTotal = await _repository.Query<MeetingRecord>()
            .CountAsync(x => x.MeetingId == meetingId, cancellationToken).ConfigureAwait(false);

        await _repository.InsertAsync(new MeetingRecord
        {
            Id = meetingRecordId,
            MeetingId = meetingId,
            EgressId = egressId,
            MeetingSubId = meetingSubId,
            StartedAt = DateTimeOffset.Now,
            RecordNumber = GenerateRecordNumber(++meetingRecordTotal)
        }, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(Guid recordId, TranslationLanguage? language, CancellationToken cancellationToken)
    {
        var currentUser = await _accountDataProvider
            .GetUserAccountAsync(_currentUser.Id.Value, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (currentUser is null) throw new UnauthorizedAccessException();
        
        var meetingInfo = await (
            from meetingRecord in _repository.QueryNoTracking<MeetingRecord>()
            join meeting in _repository.QueryNoTracking<Meeting>() on meetingRecord.MeetingId equals meeting.Id
            where meetingRecord.Id == recordId
            select new GetMeetingRecordDetailsDto
            {
                Id = recordId,
                MeetingTitle = meeting.Title,
                MeetingNumber = meeting.MeetingNumber,
                MeetingStartDate = meeting.StartDate,
                MeetingEndDate = meeting.EndDate,
                Url = meetingRecord.Url
            }).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        
        meetingInfo.MeetingRecordDetails = await GetMeetingRecordDetailsTranslationAsync(recordId, language, cancellationToken).ConfigureAwait(false);

        meetingInfo.Summary = await GetMeetingSummaryAsync(meetingInfo, language, cancellationToken).ConfigureAwait(false);
        
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
        meetingRecord.EndedAt = DateTimeOffset.Now;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingSpeakDetail>> GetMeetingDetailsByRecordIdAsync(Guid meetingRecordId, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingSpeakDetail>().Where(x=>x.MeetingRecordId == meetingRecordId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<List<MeetingSpeakDetailTranslationRecord>> GetMeetingDetailsTranslationRecordAsync(
        Guid meetingRecordId, TranslationLanguage language, int? meetingSpeakDetailId = null, CancellationToken cancellationToken = default)
    {
        var query = _repository.QueryNoTracking<MeetingSpeakDetailTranslationRecord>()
            .Where(x => x.MeetingRecordId == meetingRecordId && x.Language == language);

        if (meetingSpeakDetailId.HasValue)
            query = query.Where(x => x.MeetingSpeakDetailId == meetingSpeakDetailId.Value);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddMeetingDetailsTranslationRecordAsync(List<MeetingSpeakDetailTranslationRecord> meetingSpeakDetails, CancellationToken cancellationToken)
    {
        await _repository.InsertAllAsync(meetingSpeakDetails, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateMeetingDetailTranslationRecordAsync(MeetingSpeakDetailTranslationRecord meetingSpeakDetail, CancellationToken cancellationToken)
    {
        await _repository.UpdateAsync(meetingSpeakDetail, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateMeetingChatVoiceRecordAsync(MeetingChatVoiceRecord meetingChatVoiceRecord, bool forSave = true, CancellationToken cancellationToken = default)
    {
        if (meetingChatVoiceRecord is null) return;
        
        await _repository.UpdateAsync(meetingChatVoiceRecord, cancellationToken).ConfigureAwait(false);
        
        if (forSave) await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddMeetingRecordVoiceRelayStationAsync(
        MeetingRestartRecord meetingRestartRecord, bool forSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAsync(meetingRestartRecord, cancellationToken).ConfigureAwait(false);

        if (forSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingRestartRecord>> GetMeetingRecordVoiceRelayStationAsync(
        Guid meetingId, Guid recordId, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingRestartRecord>()
            .Where(x => x.MeetingId == meetingId && x.RecordId == recordId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }
    
    private async Task<List<MeetingSpeakDetailDto>> GetMeetingRecordDetailsTranslationAsync(Guid recordId, TranslationLanguage? language, CancellationToken cancellationToken)
    {
        var meetingRecordDetailQuery =  _repository.QueryNoTracking<MeetingSpeakDetail>()
            .Where(x => x.MeetingRecordId == recordId)
            .OrderBy(x => x.SpeakStartTime)
            .ProjectTo<MeetingSpeakDetailDto>(_mapper.ConfigurationProvider);

        if (language.HasValue)
            meetingRecordDetailQuery = from speak in meetingRecordDetailQuery
                join translation in _repository.QueryNoTracking<MeetingSpeakDetailTranslationRecord>().Where(x => x.Language == language.Value)
                    on speak.Id equals translation.MeetingSpeakDetailId into translations
                from translation in translations.DefaultIfEmpty()
                select new MeetingSpeakDetailDto
                {
                    Id = speak.Id,
                    UserId = speak.UserId,
                    TrackId = speak.TrackId,
                    Username = speak.Username,
                    SpeakStatus = speak.SpeakStatus,
                    SpeakEndTime = speak.SpeakEndTime,
                    SmartContent = speak.SmartContent,
                    MeetingNumber = speak.MeetingNumber,
                    SpeakStartTime = speak.SpeakStartTime,
                    TranslationStatus = translation == null ? 0 : translation.Status,
                    OriginalContent = speak.OriginalContent,
                    MeetingRecordId = speak.MeetingRecordId,
                    FileTranscriptionStatus = speak.FileTranscriptionStatus,
                    SmartTranslationContent = translation.SmartTranslationContent,
                    OriginalTranslationContent = translation.OriginalTranslationContent,
                    CreatedDate = speak.CreatedDate
                };

        return await meetingRecordDetailQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<MeetingSummaryDto> GetMeetingSummaryAsync(GetMeetingRecordDetailsDto meetingInfo, TranslationLanguage? language = null, CancellationToken cancellationToken = default)
    {
        var query = _repository.QueryNoTracking<MeetingSummary>()
            .Where(x => x.MeetingNumber == meetingInfo.MeetingNumber && x.RecordId == meetingInfo.Id);

        if (language.HasValue)
            query = query.Where(x => x.TargetLanguage == language);
        
        return await query.OrderByDescending(x => x.CreatedDate)
            .ProjectTo<MeetingSummaryDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<MeetingSituationDay> GetMeetingSituationDayAsync(Guid meetingId, DateTimeOffset? startTime,
        DateTimeOffset? endTime, CancellationToken cancellationToken)
    {
        var query = _repository.Query<MeetingSituationDay>().Where(x => x.MeetingId == meetingId);

        if (startTime.HasValue && endTime.HasValue)
            query = query.Where(x => x.CreatedDate >= startTime && x.CreatedDate < endTime);

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateMeetingSituationDayAsync(MeetingSituationDay meetingSituationDay, bool foreSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(meetingSituationDay, cancellationToken).ConfigureAwait(false);

        if (foreSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddMeetingSituationDayAsync(MeetingSituationDay meetingSituationDay, bool foreSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAsync(meetingSituationDay, cancellationToken).ConfigureAwait(false);

        if (foreSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<GetMeetingDataDto>> GetMeetingSituationDaysAsync(DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken)
    {
        return await (from meetingSituationDay in _repository.QueryNoTracking<MeetingSituationDay>()
            where meetingSituationDay.CreatedDate >= startTime && meetingSituationDay.CreatedDate < endTime
            join meeting in _repository.QueryNoTracking<Meeting>() on meetingSituationDay.MeetingId equals meeting.Id
            join userAccount in _repository.QueryNoTracking<UserAccount>() on meeting.CreatedBy equals userAccount.Id
            select new GetMeetingDataDto
            {
                MeetingName = meeting.Title,
                MeetingId = meeting.Id,
                MeetingNumber = meeting.MeetingNumber,
                UserId = userAccount.ThirdPartyUserId,
                MeetingCreator = userAccount.UserName,
                MeetingStartTime = meeting.StartDate,
                TimeRange = meetingSituationDay.TimePeriod,
                MeetingUseCount = meetingSituationDay.UseCount,
                MeetingDate = meetingSituationDay.CreatedDate
            }).OrderByDescending(x => x.MeetingStartTime).ToListAsync(cancellationToken);
    }

    public async Task<List<GetMeetingDataUserDto>> GetMeetingDataUserAsync(DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken)
    {
        return await (from userSession in _repository.QueryNoTracking<MeetingUserSession>()
            where userSession.CreatedDate >= startTime && userSession.CreatedDate < endTime
            join meeting in _repository.QueryNoTracking<Meeting>() on userSession.MeetingId equals meeting.Id
            join account in _repository.QueryNoTracking<UserAccount>() on userSession.UserId equals account.Id
            select new GetMeetingDataUserDto
            {
                MeetingId = meeting.Id,
                UserId = account.ThirdPartyUserId,
                UserName = account.UserName,
                MeetingNumber = meeting.MeetingNumber,
                MeetingStartTime = meeting.StartDate,
                Date = userSession.CreatedDate,
            }).OrderBy(x => x.MeetingStartTime).ToListAsync(cancellationToken);
    }

    public async Task AddMeetingRestartRecordsAsync(List<MeetingRestartRecord> meetingRestartRecords, CancellationToken cancellationToken)
    {
        await _repository.InsertAllAsync(meetingRestartRecords, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}