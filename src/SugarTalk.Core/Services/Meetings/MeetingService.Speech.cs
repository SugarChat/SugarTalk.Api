using System;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Domain.Meeting;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Smarties.Messages.Extensions;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Requests.Speech;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Enums.Account;
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
    
    Task ShouldGenerateMeetingChatVoiceRecordAsync(MeetingChatVoiceRecordDto meetingChatVoiceRecord, CancellationToken cancellationToken);
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

        var meetingSpeech = new MeetingSpeech
        {
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
                SpeechTargetLanguageType.Spanish => new TextToSpeechDto { Text = meetingSpeech.TranslatedText, SpanishToneType = userSetting?.SpanishToneType }
            };
            
            meetingSpeech.VoiceUrl = (await _speechClient.GetAudioFromTextAsync(targetLanguage, cancellationToken).ConfigureAwait(false))?.Result;
        }
    }
    
    private async Task TranslateAndGenerateTextAsync(SpeechTargetLanguageType languageType, MeetingSpeech speech, CancellationToken cancellationToken)
    {
        speech.TranslatedText = (await _speechClient.TranslateTextAsync(new TextTranslationDto
        {
            Text = speech.OriginalText,
            TargetLanguageType = languageType
        }, cancellationToken).ConfigureAwait(false))?.Result;
    }
    
    private async Task GenerateSystemVoiceUrlAsync(
        SpeechTargetLanguageType languageType, MeetingChatVoiceRecord meetingChatVoiceRecord, int voiceId, MeetingSpeech speech, CancellationToken cancellationToken)
    {
        var targetLanguage = languageType switch
            {
                SpeechTargetLanguageType.Cantonese => new TextToSpeechDto { Text = speech.TranslatedText, CantoneseToneType = (CantoneseToneType)voiceId},
                SpeechTargetLanguageType.Mandarin => new TextToSpeechDto { Text = speech.TranslatedText, MandarinToneType = (MandarinToneType)voiceId },
                SpeechTargetLanguageType.English => new TextToSpeechDto { Text = speech.TranslatedText, EnglishToneType = (EnglishToneType)voiceId },
                SpeechTargetLanguageType.Spanish => new TextToSpeechDto { Text = speech.TranslatedText, SpanishToneType = (SpanishToneType)voiceId }
            };
            
        meetingChatVoiceRecord.VoiceUrl = (await _speechClient.GetAudioFromTextAsync(targetLanguage, cancellationToken).ConfigureAwait(false))?.Result;
        
        await _meetingDataProvider.UpdateMeetingChatVoiceRecordAsync(meetingChatVoiceRecord, true, cancellationToken).ConfigureAwait(false);
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
        
        var roomSetting = await _meetingDataProvider.GetMeetingChatRoomSettingByMeetingIdAsync(
            _currentUser.Id.Value, request.MeetingId, cancellationToken).ConfigureAwait(false);
        
        if (roomSetting is null) throw new Exception("Can found roomSetting when get meeting chat voice record");
        
        Log.Information("Get meeting chat voice record roomSetting {@RoomSetting}", roomSetting);
        
        var meetingSpeeches = await _meetingDataProvider.GetMeetingSpeechesAsync(
            request.MeetingId, cancellationToken, request.FilterHasCanceledAudio).ConfigureAwait(false);

        if (meetingSpeeches is null || meetingSpeeches.Count == 0) { return new GetMeetingChatVoiceRecordEvent(); }
        
        Log.Information("Get meeting chat voice record meetingSpeeches {@MeetingSpeeches}", meetingSpeeches);

        var allSpeech = await _meetingDataProvider.GetMeetingSpeechWithVoiceRecordAsync(
            meetingSpeeches.Select(x => x.Id).ToList(), roomSetting.ListeningLanguage, cancellationToken).ConfigureAwait(false);
        
        var speechWithName = await EnhanceMeetingSpeechesWithUserNamesAsync(
            request.MeetingId, allSpeech, cancellationToken).ConfigureAwait(false);

        Log.Information("Get meeting chat voice record allSpeech {@AllSpeech}", allSpeech);
        
        var shouldGenerateVoiceRecords = speechWithName
            .Where(speech => speech.VoiceRecord == null)
            .Select(speech => new MeetingChatVoiceRecord
            {
                SpeechId = speech.Id,
                IsSelf = false,
                VoiceId = roomSetting.VoiceId,
                VoiceLanguage = roomSetting.ListeningLanguage,
                GenerationStatus = ChatRecordGenerationStatus.InProgress
            }).ToList();
        
        await _meetingDataProvider.AddMeetingChatVoiceRecordAsync(shouldGenerateVoiceRecords, true, cancellationToken).ConfigureAwait(false);

        return new GetMeetingChatVoiceRecordEvent
        {
            MeetingSpeech = speechWithName,
            ShouldGenerateVoiceRecords = _mapper.Map<List<MeetingChatVoiceRecordDto>>(shouldGenerateVoiceRecords)
        };
    }
    
    public async Task ShouldGenerateMeetingChatVoiceRecordAsync(MeetingChatVoiceRecordDto meetingChatVoiceRecord, CancellationToken cancellationToken)
    { 
        var shouldGenerateSpeech = await _meetingDataProvider.GetMeetingSpeechByIdAsync(meetingChatVoiceRecord.SpeechId, cancellationToken).ConfigureAwait(false);

        var roomSetting = await _meetingDataProvider.GetMeetingChatRoomSettingByVoiceIdAsync(meetingChatVoiceRecord.VoiceId, cancellationToken).ConfigureAwait(false);
        
        var meetingRecord = await _meetingDataProvider.GetMeetingChatVoiceRecordAsync(meetingChatVoiceRecord.Id, cancellationToken).ConfigureAwait(false);

        await GenerateChatRecordProcessAsync(meetingRecord, roomSetting, shouldGenerateSpeech, cancellationToken).ConfigureAwait(false);
    }

    public async Task GenerateChatRecordAsync(Guid meetingId, MeetingSpeech meetingSpeech, CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue) throw new UnauthorizedAccessException();
        
        var roomSetting = await _meetingDataProvider.GetMeetingChatRoomSettingByMeetingIdAsync(
            _currentUser.Id.Value, meetingId, cancellationToken).ConfigureAwait(false);
    
        var meetingRecord = new MeetingChatVoiceRecord
        {
            SpeechId = meetingSpeech.Id,
            IsSelf = true,
            VoiceId = roomSetting.VoiceId,
            VoiceLanguage = roomSetting.ListeningLanguage,
            GenerationStatus = ChatRecordGenerationStatus.InProgress
        };

        Log.Information("Generate voice Url {@MeetingRecord}", meetingRecord);
        
        await _meetingDataProvider.AddMeetingChatVoiceRecordAsync(new List<MeetingChatVoiceRecord> { meetingRecord }, true, cancellationToken).ConfigureAwait(false);

        _backgroundJobClient.Enqueue(() => GenerateChatRecordProcessAsync(meetingRecord, roomSetting, meetingSpeech, cancellationToken));
    }

    public async Task GenerateChatRecordProcessAsync(
        MeetingChatVoiceRecord meetingChatVoiceRecord, MeetingChatRoomSetting roomSetting, MeetingSpeech meetingSpeech, CancellationToken cancellationToken)
    {
        if (roomSetting.IsSystem)
        { 
            await TranslateAndGenerateTextAsync(roomSetting.ListeningLanguage, meetingSpeech, cancellationToken).ConfigureAwait(false);
            
            await GenerateSystemVoiceUrlAsync(
                roomSetting.ListeningLanguage, meetingChatVoiceRecord, int.Parse(roomSetting.VoiceId), meetingSpeech, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var voiceUrl = roomSetting.ListeningLanguage switch
            {
                SpeechTargetLanguageType.Cantonese or SpeechTargetLanguageType.Korean or SpeechTargetLanguageType.Spanish =>
                    GenerateSpeechToInferenceCantonAsync(meetingChatVoiceRecord, roomSetting, meetingSpeech, cancellationToken).Result,
                SpeechTargetLanguageType.Mandarin or SpeechTargetLanguageType.English =>
                    GenerateSpeechToInferenceMandarinAsync(meetingChatVoiceRecord, roomSetting, meetingSpeech, cancellationToken).Result,
                _ => throw new NotSupportedException(nameof(roomSetting.ListeningLanguage))
            };

            meetingChatVoiceRecord.VoiceUrl = voiceUrl; 
        
            meetingChatVoiceRecord.GenerationStatus = !string.IsNullOrEmpty(voiceUrl)
                ? ChatRecordGenerationStatus.Completed 
                : ChatRecordGenerationStatus.InProgress;
            
            await _meetingDataProvider
                .UpdateMeetingChatVoiceRecordAsync(meetingChatVoiceRecord , true, cancellationToken).ConfigureAwait(false);
        }
    }
    
    private async Task<string> GenerateSpeechToInferenceCantonAsync(
        MeetingChatVoiceRecord meetingChatVoiceRecord, MeetingChatRoomSetting roomSetting, MeetingSpeech meetingSpeech, CancellationToken cancellationToken)
    {
        var response = await _speechClient.SpeechToInferenceCantonAsync(new SpeechToInferenceCantonDto
        {
            Text = meetingSpeech.OriginalText,
            Name = roomSetting.VoiceName,
            VoiceId = meetingChatVoiceRecord.VoiceId,
            ResponseFormat = "url",
            Gender = roomSetting.Gender.GetDescription(),
            LanguageType = roomSetting.ListeningLanguage switch
            {
                SpeechTargetLanguageType.Cantonese => EchoAvatarLanguageType.English,
                SpeechTargetLanguageType.Spanish => EchoAvatarLanguageType.Spanish,
                SpeechTargetLanguageType.Korean => EchoAvatarLanguageType.Korean
            }
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("Get speech to inference canton {@Response}", response);
        
        return response?.Result?.Url;
    }

    private async Task<string> GenerateSpeechToInferenceMandarinAsync(
        MeetingChatVoiceRecord meetingChatVoiceRecord, MeetingChatRoomSetting roomSetting, MeetingSpeech meetingSpeech, CancellationToken cancellationToken)
    {
        var response = await _speechClient.SpeechToInferenceMandarinAsync(new SpeechToInferenceMandarinDto
        {
            Text = meetingSpeech.OriginalText,
            UserName = roomSetting.VoiceName,
            VoiceId = meetingChatVoiceRecord.VoiceId
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("Get speech to inference mandarin {@Response}", response);
        
        return response?.Result?.Url;
    }

    private static string HandleToBase64(string base64)
    {
        return Regex.Replace(base64, @"^data:[^;]+;[^,]+,", "");
    }
}