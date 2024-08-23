using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Messages.Dto.PostBoy;
using SugarTalk.Core.Settings.PostBoy;

namespace SugarTalk.Core.Services.Http.Clients;

public interface IPostBoyClient : IScopedDependency
{
    Task<SpeechTotextResponse> SpeechAsync(SpeechDto speechDto, CancellationToken cancellationToken);
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
    
    public async Task<SpeechTotextResponse> SpeechAsync(SpeechDto speechDto, CancellationToken cancellationToken)
    {
        return await _httpClientFactory.PostAsJsonAsync<SpeechTotextResponse>($"{_postBoySetting.BaseUrl}/api/Transcription/asr", speechDto, cancellationToken, headers: _headers).ConfigureAwait(false);
    }
}