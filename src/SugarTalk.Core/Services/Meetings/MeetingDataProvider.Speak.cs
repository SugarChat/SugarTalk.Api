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
        List<int> ids = null, string meetingNumber = null, string trackId = null, Guid? recordId = null,
        int? userId = null, SpeakStatus? speakStatus = null, CancellationToken cancellationToken = default);

    Task AddMeetingSpeakDetailAsync(MeetingSpeakDetail speakDetail, bool forceSave = true, CancellationToken cancellationToken = default);
        
    Task UpdateMeetingSpeakDetailAsync(MeetingSpeakDetail speakDetail, bool forceSave = true, CancellationToken cancellationToken = default);
    
    Task UpdateMeetingSpeakDetailsAsync(List<MeetingSpeakDetail> speakDetails, bool forceSave = true, CancellationToken cancellationToken = default);
}

public partial class MeetingDataProvider
{
    public async Task<List<MeetingSpeakDetail>> GetMeetingSpeakDetailsAsync(
        List<int> ids = null, string meetingNumber = null, string trackId = null, Guid? recordId = null,
        int? userId = null, SpeakStatus? speakStatus = null, CancellationToken cancellationToken = default)
    {
        var query = _repository.QueryNoTracking<MeetingSpeakDetail>();
        
        if (ids != null && ids.Any())
            query = query.Where(x => ids.Contains(x.Id));
        
        if (!string.IsNullOrWhiteSpace(meetingNumber))
            query = query.Where(x => x.MeetingNumber == meetingNumber);
        
        if (!string.IsNullOrWhiteSpace(trackId))
            query = query.Where(x => x.TrackId == trackId);
        
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

    public async Task UpdateMeetingSpeakDetailsAsync(
        List<MeetingSpeakDetail> speakDetails, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAllAsync(speakDetails, cancellationToken).ConfigureAwait(false);

        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}