using System;
using Serilog;
using System.Linq;
using SugarTalk.Core.Ioc;
using System.Collections.Generic;
using SugarTalk.Core.Settings.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.Core.Services.Http.Clients;

public interface IOpenAiClientBuilder : IScopedDependency
{
    Dictionary<string, string> GetRequestHeaders(OpenAiProvider provider);
}

public class OpenAiClientBuilder : IOpenAiClientBuilder
{
    private static readonly Random RandomInstance = new();

    private readonly OpenAiSettings _openAiSettings;
    private readonly IOpenAiClientKeysPool _openAiClientKeysPool;

    public OpenAiClientBuilder(OpenAiSettings openAiSettings, IOpenAiClientKeysPool openAiClientKeysPool)
    {
        _openAiSettings = openAiSettings;
        _openAiClientKeysPool = openAiClientKeysPool;
    }

    public Dictionary<string, string> GetRequestHeaders(OpenAiProvider provider)
    {
        var providerKeys = _openAiClientKeysPool.KeysPool.Where(x => x.Provider == provider).ToList();
        
        var randomIndex = RandomInstance.Next(providerKeys.Count);
        
        Log.Information("The open ai provider keys count {Count}, Random index {Index}", providerKeys.Count, randomIndex);
        
        var apiKey = providerKeys[randomIndex].ApiKey;
        var organization = providerKeys[randomIndex].Organization;
        
        return new Dictionary<string, string>
        {
            { "api-key", apiKey },
            { "Authorization", $"Bearer {apiKey}" },
            { "OpenAI-Organization", organization }
        };
    }
}