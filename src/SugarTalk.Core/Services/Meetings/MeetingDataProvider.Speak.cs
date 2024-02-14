using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Meeting.Speak;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<List<MeetingSpeakDetail>> GetMeetingSpeakDetailsAsync(
        Guid? id = null, Guid? meetingId = null, Guid? meetingSubId = null, Guid? recordId = null, int? userId = null, SpeakStatus? speakStatus = null, CancellationToken cancellationToken = default);

    Task AddMeetingSpeakDetailAsync(MeetingSpeakDetail speakDetail, bool forceSave = true, CancellationToken cancellationToken = default);
        
    Task UpdateMeetingSpeakDetailAsync(MeetingSpeakDetail speakDetail, bool forceSave = true, CancellationToken cancellationToken = default);
}

public partial class MeetingDataProvider
{
    public async Task<List<MeetingSpeakDetail>> GetMeetingSpeakDetailsAsync(
        Guid? id = null, Guid? meetingId = null, Guid? meetingSubId = null, Guid? recordId = null,
        int? userId = null, SpeakStatus? speakStatus = null, CancellationToken cancellationToken = default)
    {
        var query = _repository.QueryNoTracking<MeetingSpeakDetail>();
        
        if (id.HasValue)
            query = query.Where(x => x.Id == id.Value);
        
        if (meetingId.HasValue)
            query = query.Where(x => x.MeetingId == meetingId.Value);
        
        if (meetingSubId.HasValue)
            query = query.Where(x => x.MeetingSubId == meetingSubId.Value);
        
        if (recordId.HasValue)
            query = query.Where(x => x.MeetingRecordId == recordId.Value);
        
        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId.Value);
        
        if (speakStatus.HasValue)
            query = query.Where(x => x.SpeakStatus == speakStatus.Value);
        
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddMeetingSpeakDetailAsync(
        MeetingSpeakDetail speakDetail, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAsync(speakDetail, cancellationToken).ConfigureAwait(false);
        
        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateMeetingSpeakDetailAsync(
        MeetingSpeakDetail speakDetail, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(speakDetail, cancellationToken).ConfigureAwait(false);
        
        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}