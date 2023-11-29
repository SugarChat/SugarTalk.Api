using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task PersistMeetingSpeechAsync(MeetingSpeech meetingSpeech, CancellationToken cancellationToken);
    
    Task<List<MeetingSpeech>> GetMeetingSpeechesAsync(Guid meetingId, CancellationToken cancellationToken, bool filterHasCanceledAudio = false);
    
    Task<MeetingSpeech> GetMeetingSpeechByIdAsync(Guid meetingSpeechId, CancellationToken cancellationToken);
}

public partial class MeetingDataProvider
{
    public async Task PersistMeetingSpeechAsync(MeetingSpeech meetingSpeech, CancellationToken cancellationToken)
    {
        if (meetingSpeech is null) return;

        await _repository.InsertAsync(meetingSpeech, cancellationToken).ConfigureAwait(false);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingSpeech>> GetMeetingSpeechesAsync(
        Guid meetingId, CancellationToken cancellationToken, bool filterHasCanceledAudio = false)
    {
        var query = _repository.QueryNoTracking<MeetingSpeech>().Where(x => x.MeetingId == meetingId);
        
        if (filterHasCanceledAudio)
        {
            query = query.Where(x => x.Status != SpeechStatus.Cancelled);
        }
        
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<MeetingSpeech> GetMeetingSpeechByIdAsync(Guid meetingSpeechId, CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingSpeech>()
            .Where(x => x.Id == meetingSpeechId)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }
}