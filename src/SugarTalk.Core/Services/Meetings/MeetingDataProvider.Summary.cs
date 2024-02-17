using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Translation;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<List<MeetingSummary>> GetMeetingSummariesAsync(
        int? id = null, Guid? recordId = null, string speakIds = null, TranslationLanguage? language = null, CancellationToken cancellationToken = default);
    
    Task AddMeetingSummariesAsync(List<MeetingSummary> summaries, bool forceSave = true, CancellationToken cancellationToken = default);
    
    Task UpdateMeetingSummariesAsync(List<MeetingSummary> summaries, bool forceSave = true, CancellationToken cancellationToken = default);
}

public partial class MeetingDataProvider
{
    public async Task<List<MeetingSummary>> GetMeetingSummariesAsync(
        int? id = null, Guid? recordId = null, string speakIds = null, TranslationLanguage? language = null, CancellationToken cancellationToken = default)
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
        
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
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