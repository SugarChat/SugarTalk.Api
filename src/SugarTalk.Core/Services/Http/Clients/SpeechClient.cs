using Serilog;
using Newtonsoft.Json;
using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Settings.Speech;
using SugarTalk.Messages.Dto.Meetings.Speech;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ISpeechClient : IScopedDependency
{
    Task<SpeechResponseDto> GetTextFromAudioAsync(SpeechToTextDto speechToText, CancellationToken cancellationToken);

    Task<SpeechResponseDto> GetAudioFromTextAsync(TextToSpeechDto textToSpeech, CancellationToken cancellationToken);
    
    Task<SpeechResponseDto> TranslateTextAsync(TextTranslationDto textTranslation, CancellationToken cancellationToken);
    
    Task<SpeechToInferenceCantonResponseDto> SpeechToInferenceCantonAsync(SpeechToInferenceCantonDto speechToInference, CancellationToken cancellationToken);

    Task<SpeechToInferenceMandarinResponseDto> SpeechToInferenceMandarinAsync(SpeechToInferenceMandarinDto speechToInference, CancellationToken cancellationToken);
}

public class SpeechClient : ISpeechClient
{
    private readonly Dictionary<string, string> _headers;
    private readonly Dictionary<string, string> _echoAvatarHeader;
    
    private readonly SpeechSettings _speechSettings;
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public SpeechClient(SpeechSettings speechSettings, ISugarTalkHttpClientFactory httpClientFactory)
    {
        _speechSettings = speechSettings;
        _httpClientFactory = httpClientFactory;
        
        _headers = new Dictionary<string, string>
        {
            { "X-API-KEY", _speechSettings.Apikey }
        };
        
        _echoAvatarHeader = new Dictionary<string, string>
        {
            { "X-API-KEY", _speechSettings.EchoAvatar.Apikey }
        };
    }

    public async Task<SpeechResponseDto> GetTextFromAudioAsync(SpeechToTextDto speechToText, CancellationToken cancellationToken)
    {
        Log.Information("SugarTalk, voice turn to text :{speechToText}", JsonConvert.SerializeObject(speechToText));
        
        return await _httpClientFactory
            .PostAsJsonAsync<SpeechResponseDto>(
                $"{_speechSettings.BaseUrl}/api/speech/asr", speechToText, cancellationToken, headers: _headers).ConfigureAwait(false);
    }

    public async Task<SpeechResponseDto> GetAudioFromTextAsync(TextToSpeechDto textToSpeech, CancellationToken cancellationToken)
    {
        Log.Information("SugarTalk, text turn to voice :{textToSpeech}", JsonConvert.SerializeObject(textToSpeech));
        
        return await _httpClientFactory
            .PostAsJsonAsync<SpeechResponseDto>(
                $"{_speechSettings.BaseUrl}/api/speech/tts", textToSpeech, cancellationToken, headers: _headers).ConfigureAwait(false);
    }

    public async Task<SpeechResponseDto> TranslateTextAsync(TextTranslationDto textTranslation, CancellationToken cancellationToken)
    {
        Log.Information("SugarTalk, translate text :{textTranslation}", JsonConvert.SerializeObject(textTranslation));
        
        return await _httpClientFactory
            .PostAsJsonAsync<SpeechResponseDto>(
                $"{_speechSettings.BaseUrl}/api/speech/mt", textTranslation, cancellationToken, headers: _headers).ConfigureAwait(false);
    }
    
    public async Task<SpeechToInferenceCantonResponseDto> SpeechToInferenceCantonAsync(SpeechToInferenceCantonDto speechToInference, CancellationToken cancellationToken)
    {
        Log.Information("SugarTalk, Speech to canton:{textTranslation}", JsonConvert.SerializeObject(speechToInference));
        
        return await _httpClientFactory.PostAsJsonAsync<SpeechToInferenceCantonResponseDto>(
            $"{_speechSettings.EchoAvatar.CantonBaseUrl}/api/speech/ptts/inference/canton", speechToInference, cancellationToken, headers: _echoAvatarHeader).ConfigureAwait(false);
    }

    public async Task<SpeechToInferenceMandarinResponseDto> SpeechToInferenceMandarinAsync(SpeechToInferenceMandarinDto speechToInference, CancellationToken cancellationToken)
    {
        
        Log.Information("Speech, Speech to mandarin:{textTranslation}", JsonConvert.SerializeObject(speechToInference));

        return await _httpClientFactory.PostAsJsonAsync<SpeechToInferenceMandarinResponseDto>(
            $"{_speechSettings.EchoAvatar.BaseUrl}/api/speech/ptts/inference/mandarin", speechToInference, cancellationToken, headers: _echoAvatarHeader).ConfigureAwait(false);
    }
}