using System;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Domain.Meeting;
using System.Text.RegularExpressions;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Requests.Speech;
using SugarTalk.Messages.Dto.Meetings.Speech;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<SaveMeetingAudioResponse> SaveMeetingAudioAsync(SaveMeetingAudioCommand command, CancellationToken cancellationToken);
    
    Task<GetMeetingAudioListResponse> GetMeetingAudioListAsync(GetMeetingAudioListRequest request, CancellationToken cancellationToken);
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

        Log.Information("SugarTalk response to text :{responseToText}", responseToText);

        if (responseToText is null) return result;

        var responseToTranslatedText = await _speechClient.TranslateTextAsync(new TextTranslationDto
        {
            Text = responseToText.Result,
            TargetLanguageType = command.TargetLanguageType
        }, cancellationToken).ConfigureAwait(false);
        
        Log.Information("SugarTalk response to translated text :{responseToTranslatedText}", responseToTranslatedText);

        if (responseToTranslatedText is null) return result;

        var responseToVoice = await _speechClient.GetAudioFromTextAsync(new TextToSpeechDto
        {
            Text = responseToText.Result,
            VoiceType = command.ListenedLanguageType,
            FileFormat = "wav",
            ResponseFormat = "url"
        }, cancellationToken).ConfigureAwait(false);
        
        Log.Information("SugarTalk response to voice :{responseToVoice}", responseToVoice);

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
        var updateMeetingSpeech = new List<MeetingSpeechDto>();
        
        var meetingSpeeches = await _meetingDataProvider
            .GetMeetingSpeechAsync(request.MeetingId, cancellationToken, request.FilterHasCanceledAudio).ConfigureAwait(false);

        var meetingSpeechesDto = _mapper.Map<List<MeetingSpeechDto>>(meetingSpeeches);
        
        var userIds = meetingSpeeches.Select(x => x.UserId).ToList();

        var users = await _accountDataProvider
            .GetUserAccountsAsync(userIds, cancellationToken).ConfigureAwait(false);

        var meetingSpeechDic = meetingSpeechesDto
            .OrderBy(x => x.CreatedDate)
            .ToDictionary(x => x.UserId, x => x);

        foreach (var user in users)
        {
            meetingSpeechDic.TryGetValue(user.Id, out var meetingSpeech);
            if (meetingSpeech == null) continue;
            meetingSpeech.UserName = user.UserName;
            updateMeetingSpeech.Add(meetingSpeech);
        }

        return new GetMeetingAudioListResponse { Data = updateMeetingSpeech };
    }
}