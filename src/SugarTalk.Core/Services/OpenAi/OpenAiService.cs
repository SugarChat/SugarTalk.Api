using System;
using Serilog;
using Autofac;
using AutoMapper;
using System.Text;
using System.Linq;
using TiktokenSharp;
using Newtonsoft.Json;
using System.Threading;
using OpenAI.Interfaces;
using SugarTalk.Core.Ioc;
using OpenAI.ObjectModels;
using System.Threading.Tasks;
using SugarTalk.Core.Extensions;
using System.Collections.Generic;
using Exception = System.Exception;
using SugarTalk.Messages.Dto.OpenAi;
using SugarTalk.Core.Settings.OpenAi;
using SugarTalk.Core.Services.Ffmpeg;
using SugarTalk.Messages.Enums.OpenAi;
using OpenAI.ObjectModels.RequestModels;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Dto.Meetings.Speech;

namespace SugarTalk.Core.Services.OpenAi;

public interface IOpenAiService : IScopedDependency
{
    Task<CompletionsResponseDto> ChatCompletionsAsync(
        List<CompletionsRequestMessageDto> messages, List<CompletionsRequestFunctionDto> functions = null, object functionCall = null,
        OpenAiModel model = OpenAiModel.Gpt35Turbo16K, CompletionResponseFormatDto responseFormat =null, int? maxTokens = null, double temperature = 0, bool shouldNotSendWhenTokenLimited = false, CancellationToken cancellationToken = default);
    
    Task<string> TranscriptionAsync(
        byte[] file, TranscriptionLanguage? language, long speakStartTimeVideo, long speakEndTimeVideo, TranscriptionFileType fileType = TranscriptionFileType.Wav, 
        TranscriptionResponseFormat responseFormat = TranscriptionResponseFormat.Vtt, CancellationToken cancellationToken = default);
}

public class OpenAiService : IOpenAiService
{
    private readonly IMapper _mapper;
    private readonly ILifetimeScope _scope;
    private readonly IOpenAiClient _openAiClient;
    private readonly ISpeechClient _speechClient;
    private readonly IOpenAIService _openAiService;
    private readonly IFfmpegService _ffmpegService;
    private readonly OpenAiSettings _openAiSettings;
    
    public OpenAiService(ILifetimeScope scope, IOpenAiClient openAiClient, OpenAiSettings openAiSettings, IOpenAIService openAiService, IMapper mapper,IFfmpegService ffmpegService, ISpeechClient speechClient)
    {
        _scope = scope;
        _mapper = mapper;
        _openAiClient = openAiClient;
        _speechClient = speechClient;
        _ffmpegService = ffmpegService; 
        _openAiService = openAiService;
        _openAiSettings = openAiSettings;
    }

    public async Task<CompletionsResponseDto> ChatCompletionsAsync(
        List<CompletionsRequestMessageDto> messages, List<CompletionsRequestFunctionDto> functions = null, object functionCall = null,
        OpenAiModel model = OpenAiModel.Gpt35Turbo16K, CompletionResponseFormatDto responseFormat = null, int? maxTokens = null, double temperature = 0.5, bool shouldNotSendWhenTokenLimited = false, CancellationToken cancellationToken = default) 
    {
        var (request, limitedResponse) =
            ConfigureChatCompletions(messages, functions, functionCall, model, responseFormat, maxTokens, temperature, shouldNotSendWhenTokenLimited);
        
        if (limitedResponse != null) return limitedResponse;

        var response = await CompletionsWithRetryAndProviderSwitchAsync(
            async provider => await _openAiClient.CompletionsAsync(model, provider, request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);

        if(response != null) response.ChatCompletionsRequest = request;

        return response;
    }
    
    public async Task<string> TranscriptionAsync(
        byte[] file, TranscriptionLanguage? language, long speakStartTimeVideo, long speakEndTimeVideo, TranscriptionFileType fileType = TranscriptionFileType.Wav, 
        TranscriptionResponseFormat responseFormat = TranscriptionResponseFormat.Vtt, CancellationToken cancellationToken = default)
    {
        if (file == null) return null;
        
        var audioBytes = await _ffmpegService.ConvertFileFormatAsync(file, fileType, cancellationToken).ConfigureAwait(false);
    
        var splitAudios = await _ffmpegService.SpiltAudioAsync(audioBytes, speakStartTimeVideo, speakEndTimeVideo, cancellationToken).ConfigureAwait(false);

        var reSplitAudios = await _ffmpegService.SplitAudioAsync(splitAudios.FirstOrDefault(), secondsPerAudio: 60 * 2, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        var transcriptionResult = new StringBuilder();
    
        foreach (var reSplitAudio in reSplitAudios)
        {
            /*var transcriptionResponse = await CreateTranscriptionAsync(reSplitAudio, language, fileType, responseFormat, cancellationToken).ConfigureAwait(false);*/
            var transcriptionResponse = await TransferAsync(reSplitAudio, cancellationToken).ConfigureAwait(false);
            
            transcriptionResult.Append(transcriptionResponse);
        }
    
        Log.Information("Transcription result {Transcription}", transcriptionResult.ToString());
        
        return transcriptionResult.ToString();
    }
    
    private async Task<string> TransferAsync(byte[] audio, CancellationToken cancellationToken)
    {
        var responseToText = await _speechClient.GetTextFromAudioAsync(
            new SpeechToTextDto
            {
                Source = new Source
                {
                    Base64 = new Base64EncodedAudio
                    {
                        Encoded = Convert.ToBase64String(audio),
                        FileFormat = "wav"
                    }
                },
                LanguageId = 20,
                ResponseFormat = "text"
            }, cancellationToken).ConfigureAwait(false);

        Log.Information("Sugartalk Openai response to text :{responseToText}", JsonConvert.SerializeObject(responseToText));

        return responseToText.Result;
    }
    
    private async Task<AudioTranscriptionResponseDto> CreateTranscriptionAsync(
        byte[] file, TranscriptionLanguage? language, TranscriptionFileType fileType = TranscriptionFileType.Wav, 
        TranscriptionResponseFormat responseFormat = TranscriptionResponseFormat.Vtt, CancellationToken cancellationToken = default)
    {
        var filename = $"{Guid.NewGuid()}.{fileType.ToString().ToLower()}";
        
        var fileResponseFormat = responseFormat switch
        {
            TranscriptionResponseFormat.Vtt => StaticValues.AudioStatics.ResponseFormat.Vtt,
            TranscriptionResponseFormat.Srt => StaticValues.AudioStatics.ResponseFormat.Srt,
            TranscriptionResponseFormat.Text => StaticValues.AudioStatics.ResponseFormat.Text,
            TranscriptionResponseFormat.Json => StaticValues.AudioStatics.ResponseFormat.Json,
            TranscriptionResponseFormat.VerboseJson => StaticValues.AudioStatics.ResponseFormat.Vtt
        };

        var response = await SafelyProcessRequestAsync(nameof(CreateTranscriptionAsync), async () =>
            await _openAiService.Audio.CreateTranscription(new AudioCreateTranscriptionRequest
            {
                File = file,
                FileName = filename,
                Model = Models.WhisperV1,
                ResponseFormat = fileResponseFormat,
                Language = language?.GetDescription() ?? TranscriptionLanguage.Chinese.GetDescription()
            }, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);

        Log.Information("Transcription {FileName} response {@Response}", filename, response);
        
        return _mapper.Map<AudioTranscriptionResponseDto>(response);
    }
    
    private static async Task<T> SafelyProcessRequestAsync<T>(string methodName, Func<Task<T>> func, CancellationToken cancellationToken, bool shouldThrow = false)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            return await func().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error on method: {Method}", methodName);
            
            if (shouldThrow)
                throw;
            return default;
        }
    }

    private (ChatCompletionsRequestDto Request, CompletionsResponseDto LimitedResponse) ConfigureChatCompletions(
        List<CompletionsRequestMessageDto> messages, List<CompletionsRequestFunctionDto> functions = null, object functionCall = null,
        OpenAiModel model = OpenAiModel.Gpt35Turbo16K, CompletionResponseFormatDto responseFormat = null,  int? maxTokens = null, double temperature = 0.5, bool shouldNotSendWhenTokenLimited = false, bool? stream = false)
    {
        var limitedResponse = BuildTokenLimitedResponseIfRequired(messages, model, maxTokens, shouldNotSendWhenTokenLimited);

        if (limitedResponse != null) return (null, limitedResponse);
        
        var request = new ChatCompletionsRequestDto
        {
            Messages = messages, Functions = functions, FunctionCall = functionCall, MaxTokens = maxTokens, Temperature = temperature, Stream = stream, ResponseFormat = responseFormat
        };
        
        return (request, null);
    }
    
    private async Task<CompletionsResponseDto> CompletionsWithRetryAndProviderSwitchAsync(
        Func<OpenAiProvider, Task<CompletionsResponseDto>> completionsFunc, CancellationToken cancellationToken)
    {
        const int maxRetryCount = 5;
        
        var providers = Enum.GetValues(typeof(OpenAiProvider)).Cast<OpenAiProvider>().OrderBy(p => p != _openAiSettings.Provider).ToList();
        
        foreach (var provider in providers)
        {
            for (var i = 0; i < maxRetryCount; i++)
            {
                var response = await completionsFunc(provider).ConfigureAwait(false);

                if (response != null)
                {
                    response.Provider = provider;
                   
                    return response;
                }
                
                Log.Information("Retrying with provider {Provider} after {RetryCount} unsuccessful attempts.", provider.ToString(), i + 1);
                
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(false);
            }
            
            Log.Information("Switching to next provider after {MaxRetryCount} unsuccessful attempts with provider {Provider}.", maxRetryCount, provider.ToString());
        }

        Log.Error("All attempts with all providers failed. The OpenAI request did not return any response and might be capped by rate limiting.");
        
        return null;
    }
    
    private CompletionsResponseDto BuildTokenLimitedResponseIfRequired(
        IEnumerable<CompletionsRequestMessageDto> messages, OpenAiModel model, int? maxTokens, bool shouldNotSendWhenTokenLimited)
    {
        if (!shouldNotSendWhenTokenLimited || maxTokens == null)
            return null;

        var messagesContent = messages
            .Select(x => x.Content) 
            .Aggregate((current, next) => current + next);

        var tokens = CalculateTokens(messagesContent, model);
        
        if (tokens != 0 && tokens >= maxTokens)
        {
            return new CompletionsResponseDto
            {
                Choices = new List<CompletionsChoiceDto>
                {
                    new()
                    {
                        Text = "抱歉，超出字數限制，請減少後再嘗試。"
                    }
                }
            };
        }

        return null;
    }
    
    public int CalculateTokens(string messages, OpenAiModel model)
    {
        try
        {
            Log.Information("Calculating tokens for {Messages}", messages);

            var encoding = EncodingModel(model);

            var tikToken = TikToken.EncodingForModel(encoding);

            var tokens = tikToken.Encode(messages).Count;
        
            Log.Information("Calculated tokens for {Messages} is {Tokens}", messages, tokens);
        
            return tokens;
        }
        catch (Exception ex)
        {
            Log.Error("CalculateTokens error: {Error}", ex.Message);
            
            return 0;
        }
    }
    
    private static string EncodingModel(OpenAiModel model)
    {
        switch (model)
        {
            case OpenAiModel.Ada:
                return "text-ada-001";
            case OpenAiModel.Davinci:
                return "text-davinci-003";
            case OpenAiModel.Gpt35Turbo:
            case OpenAiModel.Gpt35Turbo16K:
                return "gpt-3.5-turbo";
            case OpenAiModel.Gpt40:
                return "gpt-4";
            case OpenAiModel.Gpt4032K:
                return "gpt-4-32k";
            case OpenAiModel.Gpt40Turbo:
                return "gpt-4-1106-preview";
            case OpenAiModel.Gpt40TurboVision:
                return "gpt-4-vision-preview";
            default:
                return "text-davinci-003";
        }
    }
}