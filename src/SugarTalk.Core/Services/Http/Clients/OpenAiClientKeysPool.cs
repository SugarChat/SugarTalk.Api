using SugarTalk.Core.Ioc;
using System.Collections.Generic;
using SugarTalk.Core.Settings.OpenAi;
using SugarTalk.Messages.Dto.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.Core.Services.Http.Clients;

public interface IOpenAiClientKeysPool : IScopedDependency
{
    List<OpenAiKeyDto> KeysPool { get; set; }
    
    void RemoveKeyFromPool(string key);
    
    void ResetKeysPool(List<OpenAiKeyDto> keysPool);
}

public class OpenAiClientKeysPool : IOpenAiClientKeysPool
{
    public List<OpenAiKeyDto> KeysPool { get; set; } = new();
    
    public OpenAiClientKeysPool(OpenAiSettings openAiSettings)
    {
        // OpenAi
        KeysPool.Add(new OpenAiKeyDto { ApiKey = openAiSettings.ApiKey, Organization = openAiSettings.Organization, Provider = OpenAiProvider.OpenAi });
        
        // Azure
        KeysPool.Add(new OpenAiKeyDto { ApiKey = openAiSettings.AzureApiKey, Provider = OpenAiProvider.Azure });
    }

    public void RemoveKeyFromPool(string key)
    {
        KeysPool.RemoveAll(x => x.ApiKey == key);
    }

    public void ResetKeysPool(List<OpenAiKeyDto> replaceKeysPool)
    {
        KeysPool = replaceKeysPool;
    }
}