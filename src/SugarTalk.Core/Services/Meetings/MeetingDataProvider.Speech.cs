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
    
    Task<List<MeetingSpeech>> GetMeetingSpeechAsync(Guid meetingId, CancellationToken cancellationToken, bool filterHasCanceledAudio = false);
}

public partial class MeetingDataProvider
{
    public async Task PersistMeetingSpeechAsync(MeetingSpeech meetingSpeech, CancellationToken cancellationToken)
    {
        await _repository.InsertAsync(meetingSpeech, cancellationToken).ConfigureAwait(false);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<MeetingSpeech>> GetMeetingSpeechAsync(
        Guid meetingId, CancellationToken cancellationToken, bool filterHasCanceledAudio = false)
    {
        var query = _repository.QueryNoTracking<MeetingSpeech>().Where(x => x.MeetingId == meetingId);
        
        if (filterHasCanceledAudio)
        {
            query = query.Where(x => x.Status != SpeechStatus.Cancelled);
        }
        
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}