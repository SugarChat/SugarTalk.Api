using System;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Domain.Meeting;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Requests.Speech;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Dto.Smarties;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Enums.Caching;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Events.Meeting.Speech;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<MeetingAudioSavedEvent> SaveMeetingAudioAsync(SaveMeetingAudioCommand command, CancellationToken cancellationToken);
    
    Task<GetMeetingAudioListResponse> GetMeetingAudioListAsync(GetMeetingAudioListRequest request, CancellationToken cancellationToken);
    
    Task<MeetingSpeechUpdatedEvent> UpdateMeetingSpeechAsync(UpdateMeetingSpeechCommand command, CancellationToken cancellationToken);

    Task<GetMeetingChatVoiceRecordEvent> GetMeetingChatVoiceRecordAsync(GetMeetingChatVoiceRecordRequest request, CancellationToken cancellationToken);
    
    Task ProcessGenerateMeetingChatVoiceRecordAsync(MeetingChatVoiceRecordDto meetingChatVoiceRecord, MeetingChatRoomSettingDto roomSetting, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<MeetingAudioSavedEvent> SaveMeetingAudioAsync(SaveMeetingAudioCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue) throw new UnauthorizedAccessException();

        var responseToText = await _speechClient.GetTextFromAudioAsync(
            new SpeechToTextDto
            {
                Source = new Source
                {
                    Base64 = new Base64EncodedAudio
                    {
                        Encoded = HandleToBase64(command.AudioForBase64),
                        FileFormat = "wav"
                    }
                },
                LanguageId = 20,
                ResponseFormat = "text"
            }, cancellationToken).ConfigureAwait(false);

        Log.Information("SugarTalk response to text :{responseToText}", JsonConvert.SerializeObject(responseToText));

        if (responseToText is null) return new MeetingAudioSavedEvent { Result = "Ai does not recognize the audio content" };

        var meetingSetting = await _meetingDataProvider.GetMeetingChatRoomSettingByMeetingIdAsync(_currentUser.Id.Value, command.MeetingId, cancellationToken).ConfigureAwait(false);
        
        var meetingSpeech = new MeetingSpeech
        {
            VoiceId = meetingSetting.VoiceId,
            MeetingId = command.MeetingId,
            UserId = _currentUser.Id.Value,
            OriginalText = responseToText.Result
        };
        
        await _meetingDataProvider.PersistMeetingSpeechAsync(meetingSpeech, cancellationToken).ConfigureAwait(false);
        
        await GenerateChatRecordAsync(command.MeetingId, meetingSpeech, cancellationToken).ConfigureAwait(false);
        
        return new MeetingAudioSavedEvent();
    }

    public async Task<GetMeetingAudioListResponse> GetMeetingAudioListAsync(GetMeetingAudioListRequest request, CancellationToken cancellationToken)
    {
        var meetingSpeeches = await _meetingDataProvider
            .GetMeetingSpeechesAsync(request.MeetingId, cancellationToken, request.FilterHasCanceledAudio).ConfigureAwait(false);

        if (meetingSpeeches is not { Count: > 0 }) return new GetMeetingAudioListResponse();

        var meetingSpeechesDto = _mapper.Map<List<MeetingSpeechDto>>(meetingSpeeches);
        
        var userIds = meetingSpeeches.Select(x => x.UserId).ToList();

        var users = await _accountDataProvider
            .GetUserAccountsAsync(userIds, cancellationToken).ConfigureAwait(false);

        var userIdsFromGuest = users.Where(x => x.Issuer == UserAccountIssuer.Guest).Select(x => x.Id).ToList();

        meetingSpeechesDto = meetingSpeechesDto.OrderBy(x => x.CreatedDate).ToList();

        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(request.MeetingId, cancellationToken).ConfigureAwait(false);

        var userSessions = (await GetMeetingByNumberAsync(new GetMeetingByNumberRequest
        {
            MeetingNumber = meeting.MeetingNumber,
            IncludeUserSession = true
        }, cancellationToken).ConfigureAwait(false)).Data?.UserSessions;

        if (userSessions is not { Count: > 0 }) return new GetMeetingAudioListResponse();

        var userSessionDicByUserId = userSessions
            .GroupBy(x => x.UserId)
            .Select(g => g.First())
            .ToDictionary(x => x.UserId, x => x);
        
        foreach (var meetingSpeech in meetingSpeechesDto)
        {
            userSessionDicByUserId.TryGetValue(meetingSpeech.UserId, out var session);

            if (session is null) continue;

            meetingSpeech.UserName = userIdsFromGuest.Contains(session.UserId)
                ? session.GuestName
                : session.UserName;
        }

        await GenerateTextByTranslateAsync(request.LanguageType, meetingSpeechesDto, cancellationToken).ConfigureAwait(false);

        await GenerateVoiceByLanguageTypeAsync(request.LanguageType, request.MeetingId, meetingSpeechesDto, cancellationToken).ConfigureAwait(false);

        return new GetMeetingAudioListResponse { Data = meetingSpeechesDto };
    }

    public async Task<List<MeetingSpeechDto>> EnhanceMeetingSpeechesWithUserNamesAsync(Guid meetingId, List<MeetingSpeechDto> meetingSpeeches, CancellationToken cancellationToken)
    {
        var userIds = meetingSpeeches.Select(x => x.UserId).ToList();

        var users = await _accountDataProvider
            .GetUserAccountsAsync(userIds, cancellationToken).ConfigureAwait(false);

        var userIdsFromGuest = users.Where(x => x.Issuer == UserAccountIssuer.Guest).Select(x => x.Id).ToList();

        meetingSpeeches = meetingSpeeches.OrderBy(x => x.CreatedDate).ToList();

        var meeting = await _meetingDataProvider.GetMeetingByIdAsync(meetingId, cancellationToken).ConfigureAwait(false);

        var userSessions = (await GetMeetingByNumberAsync(new GetMeetingByNumberRequest
        {
            MeetingNumber = meeting.MeetingNumber,
            IncludeUserSession = true
        }, cancellationToken).ConfigureAwait(false)).Data?.UserSessions;

        if (userSessions is not { Count: > 0 }) return null;
        
        var userSessionDicByUserId = userSessions
            .GroupBy(x => x.UserId)
            .Select(g => g.First())
            .ToDictionary(x => x.UserId, x => x);

        foreach (var meetingSpeech in meetingSpeeches)
        {
            userSessionDicByUserId.TryGetValue(meetingSpeech.UserId, out var session);

            if (session is null) continue;

            meetingSpeech.UserName = userIdsFromGuest.Contains(session.UserId)
                ? session.GuestName
                : session.UserName;
        }

        return meetingSpeeches;
    }

    private async Task GenerateTextByTranslateAsync(SpeechTargetLanguageType languageType, List<MeetingSpeechDto> meetingSpeechList, CancellationToken cancellationToken)
    {
        foreach (var meetingSpeech in meetingSpeechList)
        {
            meetingSpeech.TranslatedText = (await _speechClient.TranslateTextAsync(new TextTranslationDto
            {
                Text = meetingSpeech.OriginalText,
                TargetLanguageType = languageType
            }, cancellationToken).ConfigureAwait(false))?.Result;
        }
    }
    
    private async Task GenerateVoiceByLanguageTypeAsync(
        SpeechTargetLanguageType languageType, Guid meetingId, List<MeetingSpeechDto> meetingSpeechList, CancellationToken cancellationToken)
    {
        var userSettings = await _meetingDataProvider
            .GetMeetingUserSettingsAsync(meetingId, cancellationToken).ConfigureAwait(false);

        if (userSettings is null) return;

        foreach (var meetingSpeech in meetingSpeechList)
        {
            var userSetting = userSettings.FirstOrDefault(x=>x.UserId == meetingSpeech.UserId);

            var targetLanguage = languageType switch
            {
                SpeechTargetLanguageType.Cantonese => new TextToSpeechDto { Text = meetingSpeech.TranslatedText, CantoneseToneType = userSetting?.CantoneseToneType },
                SpeechTargetLanguageType.Mandarin => new TextToSpeechDto { Text = meetingSpeech.TranslatedText, MandarinToneType = userSetting?.MandarinToneType },
                SpeechTargetLanguageType.English => new TextToSpeechDto { Text = meetingSpeech.TranslatedText, EnglishToneType = userSetting?.EnglishToneType },
                SpeechTargetLanguageType.Japanese => new TextToSpeechDto { Text = meetingSpeech.TranslatedText, JapaneseToneType = userSetting?.JapaneseToneType },
                SpeechTargetLanguageType.Korean => new TextToSpeechDto { Text = meetingSpeech.TranslatedText, KoreanToneType = userSetting?.KoreanToneType },
                SpeechTargetLanguageType.Spanish => new TextToSpeechDto { Text = meetingSpeech.TranslatedText, SpanishToneType = userSetting?.SpanishToneType },
                SpeechTargetLanguageType.French => new TextToSpeechDto { Text = meetingSpeech.TranslatedText, FrenchToneType = userSetting?.FrenchToneType },
            };
            
            meetingSpeech.VoiceUrl = (await _speechClient.GetAudioFromTextAsync(targetLanguage, cancellationToken).ConfigureAwait(false))?.Result;
        }
    }
    
    private async Task TranslateAndGenerateTextAsync(SpeechTargetLanguageType languageType, MeetingChatVoiceRecord record, MeetingSpeech speech, CancellationToken cancellationToken)
    {
        record.TranslatedText = await GenerateTranslateAsync(languageType, speech, cancellationToken).ConfigureAwait(false);

        await _meetingDataProvider.UpdateMeetingChatVoiceRecordAsync(record, true, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> GenerateTranslateAsync(SpeechTargetLanguageType languageType, MeetingSpeech speech, CancellationToken cancellationToken)
    {
        return languageType switch
        {
            SpeechTargetLanguageType.Cantonese => (await _translationClient.TranslateTextAsync(speech.OriginalText, "zh-CN", cancellationToken: cancellationToken).ConfigureAwait(false))?.TranslatedText,
            _ => (await _speechClient.TranslateTextAsync(new TextTranslationDto
            {
                Text = speech.OriginalText,
                TargetLanguageType = languageType
            }, cancellationToken).ConfigureAwait(false)).Result
        };
    }

    private async Task GenerateSystemVoiceUrlAsync(MeetingChatVoiceRecord record, int voiceId, bool isSpecifyVoice, CancellationToken cancellationToken)
    {
        Log.Information("Start generating system voice url");

        var textToSpeech = BuildTextToSpeech(record, voiceId, isSpecifyVoice);
        
        record.VoiceUrl = (await _speechClient.GetAudioFromTextAsync(textToSpeech, cancellationToken).ConfigureAwait(false))?.Result;
        
        record.GenerationStatus = !string.IsNullOrEmpty(record.VoiceUrl)
            ? ChatRecordGenerationStatus.Completed 
            : ChatRecordGenerationStatus.InProgress;
        
        Log.Information("Generated system voice url{@MeetingChatVoiceRecord}", record);
        
        await _meetingDataProvider.UpdateMeetingChatVoiceRecordAsync(record, true, cancellationToken).ConfigureAwait(false);
    }

    private static TextToSpeechDto BuildTextToSpeech(MeetingChatVoiceRecord record, int voiceId, bool isSpecifyVoice) =>
        record.VoiceLanguage switch
        {
            SpeechTargetLanguageType.Cantonese => new TextToSpeechDto { Text = record.TranslatedText, CantoneseToneType = isSpecifyVoice ? (CantoneseToneType)voiceId : GetRandomEnumValue<CantoneseToneType>() },
            SpeechTargetLanguageType.Mandarin => new TextToSpeechDto { Text = record.TranslatedText, MandarinToneType = isSpecifyVoice ? (MandarinToneType)voiceId : GetRandomEnumValue<MandarinToneType>() },
            SpeechTargetLanguageType.English => new TextToSpeechDto { Text = record.TranslatedText, EnglishToneType = isSpecifyVoice ? (EnglishToneType)voiceId : GetRandomEnumValue<EnglishToneType>() },
            SpeechTargetLanguageType.Japanese => new TextToSpeechDto { Text = record.TranslatedText, JapaneseToneType = isSpecifyVoice ? (JapaneseToneType)voiceId : GetRandomEnumValue<JapaneseToneType>() },
            SpeechTargetLanguageType.Spanish => new TextToSpeechDto { Text = record.TranslatedText, SpanishToneType = isSpecifyVoice ? (SpanishToneType)voiceId : GetRandomEnumValue<SpanishToneType>() },
            SpeechTargetLanguageType.Korean => new TextToSpeechDto { Text = record.TranslatedText, KoreanToneType = isSpecifyVoice ? (KoreanToneType)voiceId : GetRandomEnumValue<KoreanToneType>() },
            SpeechTargetLanguageType.French => new TextToSpeechDto { Text = record.TranslatedText, FrenchToneType = isSpecifyVoice ? (FrenchToneType)voiceId : GetRandomEnumValue<FrenchToneType>() }
        };
    
    private static T GetRandomEnumValue<T>() where T : Enum
    {
        var random = new Random();
        var values = Enum.GetValues(typeof(T));
        var randomValue = (T)values.GetValue(random.Next(values.Length));
        return randomValue;
    }

    public async Task<MeetingSpeechUpdatedEvent> UpdateMeetingSpeechAsync(
        UpdateMeetingSpeechCommand command, CancellationToken cancellationToken)
    {
        var meetingSpeech = await _meetingDataProvider
            .GetMeetingSpeechByIdAsync(command.MeetingSpeechId, cancellationToken).ConfigureAwait(false);

        if (meetingSpeech is null) return new MeetingSpeechUpdatedEvent { Result = "unsuccessful" };

        meetingSpeech.Status = command.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new MeetingSpeechUpdatedEvent { Result = "success" };
    }

    public async Task<GetMeetingChatVoiceRecordEvent> GetMeetingChatVoiceRecordAsync(GetMeetingChatVoiceRecordRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue) throw new UnauthorizedAccessException();

        var contextId = GenerateContextId(_currentUser.Name, request.MeetingId.ToString());
        var context = await _cacheManager.GetOrAddAsync(contextId,
            () => Task.FromResult(new MeetingSpeechContext(contextId)), cachingType: CachingType.RedisCache, cancellationToken: cancellationToken).ConfigureAwait(false);
        Log.Information("Get meeting chat voice record context {@Context}", context);

        var roomSetting = await _meetingDataProvider.GetMeetingChatRoomSettingByMeetingIdAsync(
            _currentUser.Id.Value, request.MeetingId, cancellationToken).ConfigureAwait(false);
        
        if (roomSetting is null) throw new Exception("Can found roomSetting when get meeting chat voice record");
        
        Log.Information("Get meeting chat voice record roomSetting {@RoomSetting}", roomSetting);
        
        var meetingSpeeches = await _meetingDataProvider.GetMeetingSpeechesAsync(
            request.MeetingId, cancellationToken, request.FilterHasCanceledAudio).ConfigureAwait(false);

        if (meetingSpeeches is null || meetingSpeeches.Count == 0) { return new GetMeetingChatVoiceRecordEvent(); }
        
        Log.Information("Get meeting chat voice record meetingSpeeches {@MeetingSpeeches}", meetingSpeeches);
        
        var historySpeeches = await _meetingDataProvider.GetMeetingSpeechWithVoiceRecordAsync(
            context.PreviousSpeechs.Select(x => x.SpeechId).ToList(), context.PreviousSpeechs.Select(x => x.VoiceRecordId).ToList(), cancellationToken: cancellationToken).ConfigureAwait(false);
        Log.Information("Get meeting chat voice record historySpeeches {@HistorySpeeches}", historySpeeches);
        
        var currentSpeeches = await _meetingDataProvider.GetMeetingSpeechWithVoiceRecordAsync(
            meetingSpeeches.Select(x => x.Id).Except(context.PreviousSpeechs.Select(x => x.SpeechId)).ToList(), targetLanguageType: roomSetting.ListeningLanguage, cancellationToken: cancellationToken).ConfigureAwait(false);
        Log.Information("Get meeting chat voice record currentSpeeches {@CurrentSpeeches}", currentSpeeches);
        
        var allSpeech = historySpeeches.Concat(currentSpeeches).ToList();
        Log.Information("Get meeting chat voice record allSpeech {@AllSpeech}", allSpeech);
        
        var speechWithName = await EnhanceMeetingSpeechesWithUserNamesAsync(
            request.MeetingId, allSpeech, cancellationToken).ConfigureAwait(false);

        Log.Information("Get meeting chat voice record speechWithName {@SpeechWithName}", speechWithName);
        
        var shouldGenerateVoiceRecords = speechWithName
            .Where(speech => speech.VoiceRecord == null)
            .Select(speech => new MeetingChatVoiceRecord
            {
                Id = Guid.NewGuid(),
                SpeechId = speech.Id,
                IsSelf = false,
                VoiceId = speech.VoiceId,
                VoiceLanguage = roomSetting.ListeningLanguage,
                InferenceRecordId = roomSetting.InferenceRecordId,
                GenerationStatus = ChatRecordGenerationStatus.InProgress
            }).ToList();
        
        await _meetingDataProvider.AddMeetingChatVoiceRecordAsync(shouldGenerateVoiceRecords, true, cancellationToken).ConfigureAwait(false);
        context.PreviousSpeechs = allSpeech.Select(x => new SpeechWithVoiceRecord
        {
            SpeechId = x.Id,
            VoiceRecordId = x.VoiceRecord?.Id ?? shouldGenerateVoiceRecords.FirstOrDefault(record => record.SpeechId == x.Id)?.Id ?? new Guid()
        }).ToList();
        await _cacheManager.SetAsync(context.ContextId, context, CachingType.RedisCache, expiry: TimeSpan.FromDays(30), cancellationToken).ConfigureAwait(false);
        
        return new GetMeetingChatVoiceRecordEvent
        {
            MeetingSpeech = speechWithName,
            RoomSetting = _mapper.Map<MeetingChatRoomSettingDto>(roomSetting),
            ShouldGenerateVoiceRecords = _mapper.Map<List<MeetingChatVoiceRecordDto>>(shouldGenerateVoiceRecords)
        };
    }
    
    public async Task ProcessGenerateMeetingChatVoiceRecordAsync(MeetingChatVoiceRecordDto meetingChatVoiceRecord, MeetingChatRoomSettingDto roomSetting, CancellationToken cancellationToken)
    { 
        var shouldGenerateSpeech = await _meetingDataProvider.GetMeetingSpeechByIdAsync(meetingChatVoiceRecord.SpeechId, cancellationToken).ConfigureAwait(false);
        
        var meetingRecord = await _meetingDataProvider.GetMeetingChatVoiceRecordAsync(meetingChatVoiceRecord.Id, cancellationToken).ConfigureAwait(false);
        
        var meetingChatRoomSetting = await _meetingDataProvider.GetMeetingChatRoomSettingByVoiceIdAsync(meetingChatVoiceRecord.VoiceId, cancellationToken).ConfigureAwait(false);
        meetingChatRoomSetting.ListeningLanguage = meetingChatVoiceRecord.VoiceLanguage;

        await GenerateChatRecordProcessAsync(meetingRecord, meetingChatRoomSetting, shouldGenerateSpeech, false, cancellationToken).ConfigureAwait(false);
    }

    public async Task GenerateChatRecordAsync(Guid meetingId, MeetingSpeech meetingSpeech, CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue) throw new UnauthorizedAccessException();
        
        var roomSetting = await _meetingDataProvider.GetMeetingChatRoomSettingByMeetingIdAsync(
            _currentUser.Id.Value, meetingId, cancellationToken).ConfigureAwait(false);
    
        var meetingRecord = new MeetingChatVoiceRecord
        {
            IsSelf = true,
            SpeechId = meetingSpeech.Id,
            VoiceId = roomSetting.VoiceId,
            VoiceLanguage = roomSetting.ListeningLanguage,
            InferenceRecordId = roomSetting.InferenceRecordId,
            GenerationStatus = ChatRecordGenerationStatus.InProgress
        };

        Log.Information("Generate voice Url {@MeetingRecord}", meetingRecord);
        
        await _meetingDataProvider.AddMeetingChatVoiceRecordAsync(new List<MeetingChatVoiceRecord> { meetingRecord }, true, cancellationToken).ConfigureAwait(false);

        _backgroundJobClient.Enqueue(() => GenerateChatRecordProcessAsync(meetingRecord, roomSetting, meetingSpeech, true,  cancellationToken));
    }

    public async Task GenerateChatRecordProcessAsync(
        MeetingChatVoiceRecord meetingChatVoiceRecord, MeetingChatRoomSetting roomSetting, MeetingSpeech meetingSpeech, bool isSpeacifyVoice, CancellationToken cancellationToken = default)
    {
        Log.Information($"Generate Chat Record Process Room setting and record: {roomSetting}, record: {meetingChatVoiceRecord}",JsonConvert.SerializeObject(roomSetting), JsonConvert.SerializeObject(meetingChatVoiceRecord)) ;
        
        await TranslateAndGenerateTextAsync(roomSetting.ListeningLanguage, meetingChatVoiceRecord, meetingSpeech, cancellationToken).ConfigureAwait(false);
        
        if (roomSetting.IsSystem)
        {
            await GenerateSystemVoiceUrlAsync(meetingChatVoiceRecord, int.Parse(roomSetting.VoiceId), isSpeacifyVoice, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            meetingChatVoiceRecord.VoiceUrl = await ProcessSpeechInferenceAsync(roomSetting, meetingChatVoiceRecord.TranslatedText, cancellationToken).ConfigureAwait(false);
            
            meetingChatVoiceRecord.GenerationStatus = !string.IsNullOrEmpty(meetingChatVoiceRecord.VoiceUrl)
                ? ChatRecordGenerationStatus.Completed 
                : ChatRecordGenerationStatus.InProgress;
            
            await _meetingDataProvider
                .UpdateMeetingChatVoiceRecordAsync(meetingChatVoiceRecord , true, cancellationToken).ConfigureAwait(false);
        }
    }
    
    private async Task<string> ProcessSpeechInferenceAsync(MeetingChatRoomSetting roomSetting, string translatedText, CancellationToken cancellationToken)
    {
        if(!roomSetting.Transpose.HasValue || !roomSetting.Speed.HasValue || !roomSetting.Style.HasValue)
            throw new Exception("Room setting is not valid for speech inference");
        
        var users = await _accountDataProvider.GetUserAccountsAsync(roomSetting.UserId, cancellationToken).ConfigureAwait(false);

        var languageType = SpeechTargetLanguageTypeMappingToEchoAvatarLanguageType(roomSetting.ListeningLanguage);
        var voiceSetting = await _smartiesClient.GetEchoAvatarVoiceSettingAsync(new GetEchoAvatarVoiceSettingRequestDto
        {
            UserName = users.First().UserName,
            VoiceUuid = Guid.Parse(roomSetting.VoiceId),
            LanguageType = languageType
        }, cancellationToken).ConfigureAwait(false);

        var response = await _speechClient.SpeechInferenceAsync(new SpeechInferenceDto
        {
            Name = roomSetting.VoiceId,
            Text = translatedText,
            LanguageId = voiceSetting.Data.InferenceRecords.First(x => x.Language == languageType).Style,
            Transpose = roomSetting.Transpose.Value,
            Speed = roomSetting.Speed.Value,
            ResponseFormat = "url"
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("Get speech to inference {@Response}", response);
        
        return response?.Result?.Url;
    }

    private static string HandleToBase64(string base64)
    {
        return Regex.Replace(base64, @"^data:[^;]+;[^,]+,", "");
    }

    private static EchoAvatarLanguageType SpeechTargetLanguageTypeMappingToEchoAvatarLanguageType(SpeechTargetLanguageType speechTargetLanguageType)
    {
        return speechTargetLanguageType switch
        {
            SpeechTargetLanguageType.Cantonese => EchoAvatarLanguageType.Cantonese,
            SpeechTargetLanguageType.Mandarin => EchoAvatarLanguageType.Mandarin,
            SpeechTargetLanguageType.English => EchoAvatarLanguageType.English,
            SpeechTargetLanguageType.Korean => EchoAvatarLanguageType.Korean,
            SpeechTargetLanguageType.Spanish => EchoAvatarLanguageType.Spanish
        };
    }
}