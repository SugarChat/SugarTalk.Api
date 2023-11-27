using Serilog;
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
}

public class SpeechClient : ISpeechClient
{
    private readonly Dictionary<string, string> _headers;
    
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
    }

    public async Task<SpeechResponseDto> GetTextFromAudioAsync(SpeechToTextDto speechToText, CancellationToken cancellationToken)
    {
        Log.Information("SugarTalk, voice turn to text :{speechToText}", speechToText);
        
        return await _httpClientFactory
            .PostAsJsonAsync<SpeechResponseDto>(
                $"{_speechSettings.BaseUrl}/api/speech/asr", speechToText, cancellationToken, headers: _headers).ConfigureAwait(false);
    }

    public async Task<SpeechResponseDto> GetAudioFromTextAsync(TextToSpeechDto textToSpeech, CancellationToken cancellationToken)
    {
        Log.Information("SugarTalk, text turn to voice :{textToSpeech}", textToSpeech);
        
        return await _httpClientFactory
            .PostAsJsonAsync<SpeechResponseDto>(
                $"{_speechSettings.BaseUrl}/api/speech/tts", textToSpeech, cancellationToken, headers: _headers).ConfigureAwait(false);
    }

    public async Task<SpeechResponseDto> TranslateTextAsync(TextTranslationDto textTranslation, CancellationToken cancellationToken)
    {
        Log.Information("SugarTalk, translate text :{textTranslation}", textTranslation);
        
        return await _httpClientFactory
            .PostAsJsonAsync<SpeechResponseDto>(
                $"{_speechSettings.BaseUrl}/api/speech/mt", textTranslation, cancellationToken, headers: _headers).ConfigureAwait(false);
    }
}