using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using SugarTalk.Core.Extensions;
using System.Collections.Generic;
using SugarTalk.Messages.Dto.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.Core.Services.Http.Clients;

public interface IOpenAiClient : IScopedDependency
{
    Task<string> CreateTranscriptionAsync(CreateTranscriptionRequestDto request, CancellationToken cancellationToken);
}

public class OpenAiClient : IOpenAiClient
{
    private readonly IOpenAiClientBuilder _openAiClientBuilder;
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public OpenAiClient(IOpenAiClientBuilder openAiClientBuilder, ISugarTalkHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _openAiClientBuilder = openAiClientBuilder;
    }
    
    public async Task<string> CreateTranscriptionAsync(CreateTranscriptionRequestDto request, CancellationToken cancellationToken)
    {
        var headers = _openAiClientBuilder.GetRequestHeaders(OpenAiProvider.OpenAi);
        
        var parameters = new Dictionary<string, string>
        {
            { "model", request.Model },
            { "response_format", request.ResponseFormat },
            { "language", request.Language.GetDescription() }
        };

        if (!string.IsNullOrEmpty(request.Prompt))
            parameters.Add("prompt", request.Prompt);
        
        var file = new Dictionary<string, (byte[], string)> { { "file", (request.File, request.FileName) } };
        
        return await _httpClientFactory.PostAsMultipartAsync<string>("https://api.openai.com/v1/audio/transcriptions", 
            parameters, file, cancellationToken, headers: headers).ConfigureAwait(false);
    }
}