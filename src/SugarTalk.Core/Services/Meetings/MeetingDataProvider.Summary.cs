using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Translation;
using SugarTalk.Messages.Dto.Meetings.Summary;
using SugarTalk.Messages.Enums.Meeting.Summary;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<List<MeetingSummary>> GetMeetingSummariesAsync(
        int? id = null, Guid? recordId = null, string speakIds = null, TranslationLanguage? language = null, SummaryStatus? status = null, CancellationToken cancellationToken = default);

    Task<MeetingSummaryBaseInfoDto> GetMeetingSummaryBaseBySummaryIdInfoAsync(int meetingSummaryId ,CancellationToken cancellationToken);
    
    Task AddMeetingSummariesAsync(List<MeetingSummary> summaries, bool forceSave = true, CancellationToken cancellationToken = default);
    
    Task UpdateMeetingSummariesAsync(List<MeetingSummary> summaries, bool forceSave = true, CancellationToken cancellationToken = default);
}

public partial class MeetingDataProvider
{
    public async Task<List<MeetingSummary>> GetMeetingSummariesAsync(
        int? id = null, Guid? recordId = null, string speakIds = null, TranslationLanguage? language = null, SummaryStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _repository.Query<MeetingSummary>();

        if (id.HasValue)
            query = query.Where(x => x.Id == id.Value);
        
        if (recordId.HasValue)
            query = query.Where(x => x.RecordId == recordId.Value);

        if (!string.IsNullOrEmpty(speakIds))
            query = query.Where(x => x.SpeakIds == speakIds);
        
        if (language.HasValue)
            query = query.Where(x => x.TargetLanguage == language.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<MeetingSummaryBaseInfoDto> GetMeetingSummaryBaseBySummaryIdInfoAsync(int meetingSummaryId, CancellationToken cancellationToken)
    {
        var query =
            from summary in _repository.Query<MeetingSummary>().Where(x => x.Id == meetingSummaryId)
            join record in _repository.Query<MeetingRecord>() on summary.RecordId equals record.Id
            join meeting in _repository.Query<Meeting>() on record.MeetingId equals meeting.Id
            join user in _repository.Query<UserAccount>() on meeting.MeetingMasterUserId equals user.Id
            select new MeetingSummaryBaseInfoDto
            {
                MeetingTitle = meeting.Title,
                MeetingAdmin = user.UserName,
                MeetingDate = record.CreatedDate,
                MeetingRecord = summary.OriginText
            };

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddMeetingSummariesAsync(List<MeetingSummary> summaries, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAllAsync(summaries, cancellationToken).ConfigureAwait(false);
        
        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateMeetingSummariesAsync(List<MeetingSummary> summaries, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAllAsync(summaries, cancellationToken).ConfigureAwait(false);
        
        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}