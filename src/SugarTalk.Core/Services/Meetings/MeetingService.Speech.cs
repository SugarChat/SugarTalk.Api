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
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<SaveMeetingAudioResponse> SaveMeetingAudioAsync(SaveMeetingAudioCommand command, CancellationToken cancellationToken);
    
    Task<GetMeetingAudioListResponse> GetMeetingAudioListAsync(GetMeetingAudioListRequest request, CancellationToken cancellationToken);
    
    Task<UpdateMeetingAudioResponse> UpdateMeetingSpeechAsync(UpdateMeetingSpeechCommand command, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<SaveMeetingAudioResponse> SaveMeetingAudioAsync(SaveMeetingAudioCommand command, CancellationToken cancellationToken)
    {
        var result = new SaveMeetingAudioResponse { Data = "Unsuccessful" };
        
        if (!_currentUser.Id.HasValue) throw new UnauthorizedAccessException();
        
        var responseToText = await _speechClient.GetTextFromAudioAsync(new SpeechToTextDto
        {
            Source = new Source
            {
                Base64 = new Base64
                {
                    Encoded = HandleToBase64(command.AudioForBase64),
                    FileFormat = "wav"
                }
            },
            LanguageId = 20,
            ResponseFormat = "text"
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("SugarTalk response to text :{responseToText}", JsonConvert.SerializeObject(responseToText));

        if (responseToText is null) return result;

        var userSetting = await _meetingDataProvider
            .GetMeetingUserSettingByUserIdAsync(_currentUser.Id.Value, command.MeetingId, cancellationToken).ConfigureAwait(false);

        if (userSetting is null) throw new NoFoundMeetingUserSettingForCurrentUserException(_currentUser.Id.Value);

        var speech = new MeetingSpeech
        {
            MeetingId = command.MeetingId,
            UserId = _currentUser.Id.Value,
            OriginalText = responseToText.Result
        };
        
        await _meetingDataProvider.PersistMeetingSpeechAsync(speech, cancellationToken).ConfigureAwait(false);
        
        result.Data = "Successful";
        
        return result;
    }

    public async Task<GetMeetingAudioListResponse> GetMeetingAudioListAsync(GetMeetingAudioListRequest request, CancellationToken cancellationToken)
    {
        var meetingSpeeches = await _meetingDataProvider
            .GetMeetingSpeechesAsync(request.MeetingId, cancellationToken, request.FilterHasCanceledAudio).ConfigureAwait(false);

        var meetingSpeechesDto = _mapper.Map<List<MeetingSpeechDto>>(meetingSpeeches);
        
        var userIds = meetingSpeeches.Select(x => x.UserId).ToList();

        var users = await _accountDataProvider
            .GetUserAccountsAsync(userIds, cancellationToken).ConfigureAwait(false);

        meetingSpeechesDto = meetingSpeechesDto.OrderBy(x => x.CreatedDate).ToList();

        var userDictionary = users.ToDictionary(user => user.Id, user => user);

        foreach (var meetingSpeech in meetingSpeechesDto)
        {
            if (userDictionary.TryGetValue(meetingSpeech.UserId, out var userAccount))
            {
                meetingSpeech.UserName = userAccount.UserName;
            }
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

    public async Task<UpdateMeetingAudioResponse> UpdateMeetingSpeechAsync(
        UpdateMeetingSpeechCommand command, CancellationToken cancellationToken)
    {
        var meetingSpeech = await _meetingDataProvider
            .GetMeetingSpeechByIdAsync(command.MeetingSpeechId, cancellationToken).ConfigureAwait(false);

        if (meetingSpeech is null) return new UpdateMeetingAudioResponse { Data = "unsuccessful" };

        meetingSpeech.Status = command.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new UpdateMeetingAudioResponse { Data = "success" };
    }
    
    private static string HandleToBase64(string base64)
    {
        return Regex.Replace(base64, @"^data:[^;]+;[^,]+,", "");
    }
}