using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Messages.Dto.PostBoy;
using SugarTalk.Core.Settings.PostBoy;

namespace SugarTalk.Core.Services.Http.Clients;

public interface IPostBoyClient : IScopedDependency
{
    Task<SpeechResponse> SpeechAsync(SpeechDto speechDto, CancellationToken cancellationToken);
}

public class PostBoyClient : IPostBoyClient
{
    private readonly PostBoySettings _postBoySetting;
    private readonly Dictionary<string, string> _headers;
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;
    
    public PostBoyClient(PostBoySettings postBoySetting, ISugarTalkHttpClientFactory httpClientFactory)
    {
        _postBoySetting = postBoySetting;
        _httpClientFactory = httpClientFactory;
        
        _headers = new Dictionary<string, string>
        {
            { "X-API-KEY", _postBoySetting.ApiKey }
        };
    }
    
    public async Task<SpeechResponse> SpeechAsync(SpeechDto speechDto, CancellationToken cancellationToken)
    {
        return await _httpClientFactory.PostAsJsonAsync<SpeechResponse>($"{_postBoySetting.BaseUrl}/api/Transfer/speech", speechDto, cancellationToken, headers: _headers).ConfigureAwait(false);
    }
}