using System;
using Serilog;
using System.Linq;
using TiktokenSharp;
using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Exception = System.Exception;
using SugarTalk.Messages.Dto.OpenAi;
using SugarTalk.Core.Settings.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;
using SugarTalk.Core.Services.Http.Clients;

namespace SugarTalk.Core.Services.OpenAi;

public interface IOpenAiService : IScopedDependency
{
    Task<CompletionsResponseDto> ChatCompletionsAsync(
        List<CompletionsRequestMessageDto> messages, List<CompletionsRequestFunctionDto> functions = null, object functionCall = null,
        OpenAiModel model = OpenAiModel.Gpt35Turbo16K, int? maxTokens = null, double temperature = 0, bool shouldNotSendWhenTokenLimited = false, CancellationToken cancellationToken = default);
}

public class OpenAiService : IOpenAiService
{
    private readonly IOpenAiClient _openAiClient;
    private readonly OpenAiSettings _openAiSettings;

    public OpenAiService(IOpenAiClient openAiClient, OpenAiSettings openAiSettings)
    {
        _openAiClient = openAiClient;
        _openAiSettings = openAiSettings;
    }

    public async Task<CompletionsResponseDto> ChatCompletionsAsync(
        List<CompletionsRequestMessageDto> messages, List<CompletionsRequestFunctionDto> functions = null, object functionCall = null,
        OpenAiModel model = OpenAiModel.Gpt35Turbo16K, int? maxTokens = null, double temperature = 0.5, bool shouldNotSendWhenTokenLimited = false, CancellationToken cancellationToken = default) 
    {
        var (request, limitedResponse) =
            ConfigureChatCompletions(messages, functions, functionCall, model, maxTokens, temperature, shouldNotSendWhenTokenLimited);
        
        if (limitedResponse != null) return limitedResponse;

        var response = await CompletionsWithRetryAndProviderSwitchAsync(
            async provider => await _openAiClient.CompletionsAsync(model, provider, request, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);

        if(response != null) response.ChatCompletionsRequest = request;

        return response;
    }
    
    private (ChatCompletionsRequestDto Request, CompletionsResponseDto LimitedResponse) ConfigureChatCompletions(
        List<CompletionsRequestMessageDto> messages, List<CompletionsRequestFunctionDto> functions = null, object functionCall = null,
        OpenAiModel model = OpenAiModel.Gpt35Turbo16K, int? maxTokens = null, double temperature = 0.5, bool shouldNotSendWhenTokenLimited = false, bool? stream = false)
    {
        var limitedResponse = BuildTokenLimitedResponseIfRequired(messages, model, maxTokens, shouldNotSendWhenTokenLimited);

        if (limitedResponse != null) return (null, limitedResponse);
        
        var request = new ChatCompletionsRequestDto
        {
            Messages = messages, Functions = functions, FunctionCall = functionCall, MaxTokens = maxTokens, Temperature = temperature, Stream = stream
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