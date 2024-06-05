using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task PersistMeetingSpeechAsync(MeetingSpeech meetingSpeech, CancellationToken cancellationToken);
    
    Task<List<MeetingSpeech>> GetMeetingSpeechesAsync(Guid meetingId, CancellationToken cancellationToken, bool filterHasCanceledAudio = false);
    
    Task<MeetingSpeech> GetMeetingSpeechByIdAsync(Guid meetingSpeechId, CancellationToken cancellationToken);
    
    Task<MeetingUserSetting> DistributeLanguageForMeetingUserAsync(Guid meetingId, CancellationToken cancellationToken);

    Task<List<MeetingSpeechDto>> GetMeetingSpeechWithVoiceRecordAsync(
        List<Guid> speechIds, List<Guid> recordIds = null, SpeechTargetLanguageType? targetLanguageType = null, CancellationToken cancellationToken = default);
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

        var userSettings = await _repository.QueryNoTracking<MeetingUserSetting>()
            .Where(x => x.MeetingId == meetingId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        
        var existMeetingUserSetting = userSettings.FirstOrDefault(x => x.UserId == _currentUser.Id.Value);

        if (existMeetingUserSetting != null) return existMeetingUserSetting;
        
        AssignTone(userSettings, x => x.SpanishToneType, meetingUserSetting);
        AssignTone(userSettings, x => x.EnglishToneType, meetingUserSetting);
        AssignTone(userSettings, x => x.MandarinToneType, meetingUserSetting);
        AssignTone(userSettings, x => x.CantoneseToneType, meetingUserSetting);
        AssignTone(userSettings, x => x.FrenchToneType, meetingUserSetting);
        AssignTone(userSettings, x => x.JapaneseToneType, meetingUserSetting);
        AssignTone(userSettings, x => x.KoreanToneType, meetingUserSetting);
        
        AssignCantoneseTone(meetingUserSetting);

        meetingUserSetting.MeetingId = meetingId;
        meetingUserSetting.UserId = _currentUser.Id.Value;

        await _repository.InsertAsync(meetingUserSetting, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return meetingUserSetting;
    }

    public async Task<List<MeetingSpeechDto>> GetMeetingSpeechWithVoiceRecordAsync(List<Guid> speechIds, List<Guid> recordIds = null, SpeechTargetLanguageType? targetLanguageType = null, CancellationToken cancellationToken = default)
    {
        var voiceRecordQuery = _repository.Query<MeetingChatVoiceRecord>();

        if (recordIds != null) voiceRecordQuery = voiceRecordQuery.Where(x => recordIds.Contains(x.Id));
        
        if (targetLanguageType.HasValue)
            voiceRecordQuery = voiceRecordQuery.Where(x => x.VoiceLanguage == targetLanguageType.Value);
        
        var query =
            from speech in _repository.Query<MeetingSpeech>()
            join record in voiceRecordQuery
                on speech.Id equals record.SpeechId into voiceRecordGroup
            from record in voiceRecordGroup.DefaultIfEmpty()
            where speechIds.Contains(speech.Id)
            orderby speech.CreatedDate descending
            select new MeetingSpeechDto
            {
                Id = speech.Id,
                MeetingId = speech.MeetingId,
                UserId = speech.UserId,
                OriginalText = speech.OriginalText,
                Status = speech.Status,
                CreatedDate = speech.CreatedDate,
                VoiceId = speech.VoiceId,
                VoiceRecord = record == null ? null : _mapper.Map<MeetingChatVoiceRecordDto>(record),
            };

        var groupedQuery = query.GroupBy(ms => ms.Id).Select(x => x.First());
        
        return await groupedQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private void AssignTone<T>(
        List<MeetingUserSetting> userSettings, Func<MeetingUserSetting, T> toneSelector, MeetingUserSetting meetingUserSetting) where T : Enum
    {
        var usedTones = userSettings.Select(toneSelector).Distinct().ToList();
        var allTones = Enum.GetValues(typeof(T)).Cast<T>().ToList();
        var availableTones = allTones.Except(usedTones).ToList();

        if (availableTones.Any())
        {
            SetProperty(meetingUserSetting, availableTones.First());
        }
        else
        {
            var random = new Random();
            
            var randomTone = allTones[random.Next(0, allTones.Count)];

            SetProperty(meetingUserSetting, randomTone);
        }
    }
    
    private void SetProperty<T>(MeetingUserSetting meetingUserSetting, T tone)
    {
        var property = typeof(MeetingUserSetting).GetProperty($"{typeof(T).Name}");
        property?.SetValue(meetingUserSetting, tone);
    }
    
    private void AssignCantoneseTone(MeetingUserSetting meetingUserSetting)
    {
        var random = new Random();
        
        var values = Enum.GetValues(typeof(CantoneseToneType)).Cast<CantoneseToneType>().ToList();
        
        meetingUserSetting.CantoneseToneType = values.ElementAt(random.Next(values.Count));
    }
}