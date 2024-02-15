using System;
using SugarTalk.Core.Settings.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.Core.Services.Utils;

public static class OpenAiUtil
{
    public static string GetFullUrl(OpenAiModel model, string modelString, OpenAiProvider provider, OpenAiSettings openAiSettings = null)
    {
        var baseUrl = GetBaseUrl(provider);

        return provider switch
        {
            OpenAiProvider.OpenAi => GetOpenAiUrl(model, baseUrl),
            OpenAiProvider.Azure => GetAzureUrl(model, modelString, baseUrl, openAiSettings),
            _ => throw new NotSupportedException($"Provider {provider} is not supported.")
        };
    }
    
    public static string ConvertModelToStr(OpenAiModel model, OpenAiProvider provider)
    {
        return provider switch
        {
            OpenAiProvider.Azure => ConvertModelForAzure(model), _ => ConvertModelForStandardProviders(model, provider)
        };
    }
    
    private static string GetBaseUrl(OpenAiProvider provider)
    {
        return provider switch
        {
            OpenAiProvider.OpenAi => "https://api.openai.com",
            OpenAiProvider.Azure => "https://sjswedencentralopenai.openai.azure.com",
            _ => throw new NotSupportedException($"Provider {provider} is not supported.")
        };
    }

    private static string GetOpenAiUrl(OpenAiModel model, string baseUrl)
    {
        return model switch
        {
            OpenAiModel.DallE2 or OpenAiModel.DallE3 => $"{baseUrl}/v1/images/generations",
            _ => $"{baseUrl}/v1/chat/completions"
        };
    }
    
    private static string GetProxyUrl(OpenAiModel model, string baseUrl)
    {
        return model switch
        {
            OpenAiModel.DallE2 or OpenAiModel.DallE3 => $"{baseUrl}/v1/images/generations",
            _ => $"{baseUrl}/v1/chat/completions"
        };
    }
    
    private static string GetAzureUrl(OpenAiModel model, string modelString, string baseUrl, OpenAiSettings openAiSettings)
    {
        return model switch
        {
            OpenAiModel.DallE3 => throw new NotSupportedException(nameof(model)),
            _ => $"{baseUrl}/openai/deployments/{modelString}/chat/completions?api-version={openAiSettings.AzureApiVersion}"
        };
    }

    private static string ConvertModelForAzure(OpenAiModel model)
    {
        return model switch
        {
            OpenAiModel.Gpt35Turbo => "gpt-35-turbo",
            OpenAiModel.Gpt35Turbo16K => "gpt-35-turbo-16k",
            _ => throw new NotSupportedException($"Azure provider does not support model {model}")
        };
    }

    private static string ConvertModelForStandardProviders(OpenAiModel model, OpenAiProvider provider)
    {
        return model switch
        {
            OpenAiModel.Gpt35Turbo => "gpt-3.5-turbo", 
            OpenAiModel.Gpt35Turbo16K => "gpt-3.5-turbo-16k",
            OpenAiModel.Gpt40 => "gpt-4", 
            OpenAiModel.Gpt4032K => "gpt-4-32k", 
            OpenAiModel.Gpt40Turbo => "gpt-4-1106-preview", 
            OpenAiModel.Gpt40TurboVision => "gpt-4-vision-preview",
            OpenAiModel.DallE2 => "dall-e-2",
            OpenAiModel.DallE3 => "dall-e-3",
            _ => throw new NotSupportedException($"Model {model} is not supported by {provider.ToString()} provider.")
        };
    }
}