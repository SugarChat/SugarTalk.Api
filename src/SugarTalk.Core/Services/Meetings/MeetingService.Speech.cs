using System;
using Serilog;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Domain.Meeting;
using System.Text.RegularExpressions;
using MassTransit.Futures.Contracts;
using Microsoft.IdentityModel.Tokens;
using PostBoy.Messages.Extensions;
using SugarTalk.Core.Extensions;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Requests.Speech;
using SugarTalk.Messages.Dto.Meetings.Speech;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Events.Meeting.Speech;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<MeetingAudioSavedEvent> SaveMeetingAudioAsync(SaveMeetingAudioCommand command, CancellationToken cancellationToken);
    
    Task<GetMeetingAudioListResponse> GetMeetingAudioListAsync(GetMeetingAudioListRequest request, CancellationToken cancellationToken);
    
    Task<MeetingSpeechUpdatedEvent> UpdateMeetingSpeechAsync(UpdateMeetingSpeechCommand command, CancellationToken cancellationToken);

    Task<GetMeetingVoiceListResponse> GetMeetingVoiceListAsync(GetMeetingVoiceListRequest request, CancellationToken cancellationToken);
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

    public async Task<GetMeetingVoiceListResponse> GetMeetingVoiceListAsync(GetMeetingVoiceListRequest request, CancellationToken cancellationToken)
    {
        var meetingSpeeches = await _meetingDataProvider
            .GetMeetingSpeechesAsync(request.MeetingId, cancellationToken, request.FilterHasCanceledAudio).ConfigureAwait(false);

        var meetingSpeechesDto = _mapper.Map<List<SpeechMappingDto>>(meetingSpeeches);
        
        var userIds = meetingSpeeches.Select(x => x.UserId).ToList();

        var users = await _accountDataProvider
            .GetUserAccountsAsync(userIds, cancellationToken).ConfigureAwait(false);

        meetingSpeechesDto = meetingSpeechesDto.OrderBy(x => x.CreatedDate).ToList();

        var userDictionary = users.ToDictionary(user => user.Id, user => user);
        
        var meetingSpeechUserSetting = await _accountDataProvider.GetMeetingUserSettingAsync(
            userIds, request.MeetingId, cancellationToken).ConfigureAwait(false);
        
        var settingDictionary = meetingSpeechUserSetting.ToDictionary(setting => setting.UserId, setting => setting);
        
        foreach (var meetingSpeech in meetingSpeechesDto)
        {
            if (userDictionary.TryGetValue(meetingSpeech.UserId, out var userAccount))
            {
                meetingSpeech.UserName = userAccount.UserName;
            }
            
            if (settingDictionary.TryGetValue(meetingSpeech.UserId, out var userSetting))
            {
                meetingSpeech.VoiceId = userSetting.VoiceId;
                meetingSpeech.LanguageId = userSetting.LanguageId;
            }
        }
        
        var meetingSpeechIds = meetingSpeeches.Select(x => x.Id).ToList();

        var voiceIds = meetingSpeechesDto.Select(x => x.VoiceId).ToList();

        var languageIds = meetingSpeechesDto.Select(x => x.LanguageId).ToList();
        
        var meetingSpeechesVoice = await _accountDataProvider.GetMeetingSpeechVoiceAsync(
            meetingSpeechIds, voiceIds, languageIds, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (meetingSpeechesVoice != null)
        {
            var meetingSpeechVoiceDictionary = meetingSpeechesVoice.ToDictionary(
                speech => (speech.MeetingSpeechId, speech.VoiceId, speech.LanguageId), speech => speech);
        
            foreach (var meetingSpeech in meetingSpeechesDto)
            {
                var key = (meetingSpeech.Id, meetingSpeech.VoiceId, meetingSpeech.LanguageId);
                
                if (meetingSpeechVoiceDictionary.TryGetValue(key, out var speechVoice))
                {
                    meetingSpeech.TranslateText = speechVoice.TranslateText;
                    meetingSpeech.VoiceUrl = speechVoice.VoiceUrl;
                    meetingSpeech.LoadStatus = speechVoice.Status;
                }
            }
        }
        
        var meetingSpeechesNoTranslate = meetingSpeechesDto.Where(x => 
            x.TranslateText.IsNullOrEmpty() && x.VoiceUrl.IsNullOrEmpty() && x.LoadStatus == SpeechAudioLoadStatus.Pending).ToList();
        
        meetingSpeechesVoice = meetingSpeechesNoTranslate.Select(x =>
        {
            var meetingSpeechVoice = meetingSpeechesVoice?.FirstOrDefault(s => s.MeetingSpeechId == x.Id && s.VoiceId == x.VoiceId && s.LanguageId == x.LanguageId);

            if (meetingSpeechVoice != null)
                meetingSpeechVoice.Status = SpeechAudioLoadStatus.Progress;

            return meetingSpeechVoice;
        }).ToList();

        await _accountDataProvider.UpdateMeetingSpeechVoiceTableAsync(meetingSpeechesVoice, cancellationToken).ConfigureAwait(false);

        foreach (var meetingSpeech in meetingSpeechesNoTranslate)
        {
            _backgroundJobClient.Enqueue(() => GenerateProcessVoiceAsync(meetingSpeech, cancellationToken));
        }
        
        return new GetMeetingVoiceListResponse { Data = new GetMeetingVoiceListDto
        {
            Speeches = _mapper.Map<List<SpeechDto>>(meetingSpeechesDto)
        }};
    }
    
    private async Task GenerateProcessVoiceAsync(SpeechMappingDto meetingSpeech, CancellationToken cancellationToken)
    {
        await TextByTranslateAsync(meetingSpeech, cancellationToken).ConfigureAwait(false);
        
        await VoiceByLanguageTypeAsync(meetingSpeech, cancellationToken).ConfigureAwait(false);

        var meetingSpeechVoice = (await _accountDataProvider.GetMeetingSpeechVoiceAsync(
            speechId: meetingSpeech.Id, voiceId: meetingSpeech.VoiceId, languageId: meetingSpeech.LanguageId, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (meetingSpeechVoice != null) meetingSpeechVoice.Status = SpeechAudioLoadStatus.Completed;

        await _accountDataProvider.UpdateMeetingSpeechVoiceTableAsync(new List<MeetingSpeechVoiceTable>{meetingSpeechVoice}, cancellationToken).ConfigureAwait(false);
    }

    private async Task TextByTranslateAsync(SpeechMappingDto meetingSpeech, CancellationToken cancellationToken)
    {
        meetingSpeech.TranslateText = (await _speechClient.SpeechToInferenceMandarinAsync(
            new SpeechToInferenceMandarinDto
            {
                VoiceId = meetingSpeech.VoiceId,
                UserName = meetingSpeech.UserName,
                Text = meetingSpeech.OriginalText
            }, cancellationToken).ConfigureAwait(false))?.Result.Url.UrlValue;
    }

    private async Task VoiceByLanguageTypeAsync(SpeechMappingDto meetingSpeech, CancellationToken cancellationToken)
    {
        var speechToInferenceCantonDto = new SpeechToInferenceCantonDto
        {
            ResponseFormat = "url",
            Name = meetingSpeech.UserName,
            VoiceId = meetingSpeech.VoiceId,
            Text = meetingSpeech.OriginalText,
            LanguageType = meetingSpeech.LanguageId switch
            {
                SpeechTargetLanguageType.Mandarin => EchoAvatarLanguageType.Mandarin,
                SpeechTargetLanguageType.Cantonese => EchoAvatarLanguageType.Cantonese,
                SpeechTargetLanguageType.English => EchoAvatarLanguageType.English,
                SpeechTargetLanguageType.Spanish => EchoAvatarLanguageType.Spanish,
                SpeechTargetLanguageType.Korean => EchoAvatarLanguageType.Korean,
                _ => throw new ArgumentOutOfRangeException()
            }
        };

        meetingSpeech.VoiceUrl =
            (await _speechClient.SpeechToInferenceCantonAsync(speechToInferenceCantonDto, cancellationToken).ConfigureAwait(false))?.Result.Url;
    }

    private static string HandleToBase64(string base64)
    {
        return Regex.Replace(base64, @"^data:[^;]+;[^,]+,", "");
    }
}