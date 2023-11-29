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

        var base64WithoutPrefix = Regex.Replace(command.AudioForBase64, @"^data:[^;]+;[^,]+,", "");
        
        var responseToText = await _speechClient.GetTextFromAudioAsync(new SpeechToTextDto
        {
            Source = new Source
            {
                Base64 = new Base64
                {
                    Encoded = base64WithoutPrefix,
                    FileFormat = "wav"
                }
            },
            LanguageId = 20,
            ResponseFormat = "text"
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("SugarTalk response to text :{responseToText}", JsonConvert.SerializeObject(responseToText));

        if (responseToText is null) return result;

        var userSetting = await _meetingDataProvider
            .GetMeetingUserSettingByUserIdAsync(_currentUser.Id.Value, cancellationToken).ConfigureAwait(false);

        if (userSetting is null) throw new NoFoundMeetingUserSettingForCurrentUserException(_currentUser.Id.Value);

        var responseToTranslatedText = await _speechClient.TranslateTextAsync(new TextTranslationDto
        {
            Text = responseToText.Result,
            TargetLanguageType = userSetting.TargetLanguageType
        }, cancellationToken).ConfigureAwait(false);
        
        Log.Information("SugarTalk response to translated text :{responseToTranslatedText}", JsonConvert.SerializeObject(responseToTranslatedText));

        if (responseToTranslatedText is null) return result;

        var responseToVoice = await _speechClient.GetAudioFromTextAsync(new TextToSpeechDto
        {
            Text = responseToText.Result,
            VoiceType = userSetting.ListenedLanguageType,
            FileFormat = "wav",
            ResponseFormat = "url"
        }, cancellationToken).ConfigureAwait(false);
        
        Log.Information("SugarTalk response to voice :{responseToVoice}", JsonConvert.SerializeObject(responseToVoice));

        if (responseToVoice is null) return result;

        var speech = new MeetingSpeech
        {
            MeetingId = command.MeetingId,
            UserId = _currentUser.Id.Value,
            VoiceUrl = responseToVoice.Result,
            OriginalText = responseToText.Result,
            TranslatedText = responseToTranslatedText.Result
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

        return new GetMeetingAudioListResponse { Data = meetingSpeechesDto };
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
}