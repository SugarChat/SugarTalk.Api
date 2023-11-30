using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings.User;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task PersistMeetingSpeechAsync(MeetingSpeech meetingSpeech, CancellationToken cancellationToken);
    
    Task<List<MeetingSpeech>> GetMeetingSpeechesAsync(Guid meetingId, CancellationToken cancellationToken, bool filterHasCanceledAudio = false);
    
    Task<MeetingSpeech> GetMeetingSpeechByIdAsync(Guid meetingSpeechId, CancellationToken cancellationToken);
    
    Task<MeetingUserSetting> DistributeLanguageForMeetingUserAsync(Guid meetingId, CancellationToken cancellationToken);
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

    public async Task<MeetingUserSetting> DistributeLanguageForMeetingUserAsync(Guid meetingId, CancellationToken cancellationToken)
    {
        var meetingUserSetting = new MeetingUserSetting();

        var userSettings= await _repository.QueryNoTracking<MeetingUserSetting>()
            .Where(x => x.MeetingId == meetingId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        
        //todo:
        var existMeetingUserSetting = userSettings.FirstOrDefault(x => x.UserId == _currentUser.Id.Value);

        if (existMeetingUserSetting != null) return existMeetingUserSetting;

        if (userSettings.Count > 10) return null;
        
        AssignTone(userSettings, x => x.SpanishToneType, meetingUserSetting);
        AssignTone(userSettings, x => x.EnglishToneType, meetingUserSetting);
        AssignTone(userSettings, x => x.MandarinToneType, meetingUserSetting);
        AssignCantoneseTone(meetingUserSetting);

        meetingUserSetting.MeetingId = meetingId;
        meetingUserSetting.UserId = _currentUser.Id.Value;

        await _repository.InsertAsync(meetingUserSetting, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return meetingUserSetting;
    }
    
    private void AssignTone<T>(
        List<MeetingUserSetting> userSettings, Func<MeetingUserSetting, T> toneSelector, MeetingUserSetting meetingUserSetting) where T : Enum
    {
        var usedTones = userSettings.Select(toneSelector).Distinct().ToList();
        var allTones = Enum.GetValues(typeof(T)).Cast<T>();
        var availableTones = allTones.Except(usedTones).ToList();

        if (availableTones.Any())
        {
            var property = typeof(MeetingUserSetting).GetProperty($"{typeof(T).Name}");
            
            property?.SetValue(meetingUserSetting, availableTones.First());
        }
    }
    
    private void AssignCantoneseTone(MeetingUserSetting meetingUserSetting)
    {
        var random = new Random();
        
        var values = Enum.GetValues(typeof(CantoneseToneType)).Cast<CantoneseToneType>().ToList();
        
        meetingUserSetting.CantoneseToneType = values.ElementAt(random.Next(values.Count));
    }
}