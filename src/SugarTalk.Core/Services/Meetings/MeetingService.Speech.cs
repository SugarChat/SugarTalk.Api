using System;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Domain.Meeting;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SugarTalk.Core.Services.Exceptions;
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

        await _meetingDataProvider.PersistMeetingSpeechAsync(new MeetingSpeech
        {
            MeetingId = command.MeetingId,
            UserId = _currentUser.Id.Value,
            OriginalText = responseToText.Result
        }, cancellationToken).ConfigureAwait(false);
        
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

        var userSessionDicByUserId = userSessions.ToDictionary(x => x.UserId, x => x);
        
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
    
    private static string HandleToBase64(string base64)
    {
        return Regex.Replace(base64, @"^data:[^;]+;[^,]+,", "");
    }
}