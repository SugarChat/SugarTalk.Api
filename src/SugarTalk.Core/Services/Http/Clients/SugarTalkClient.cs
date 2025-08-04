using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.SuagrTalk;
using SugarTalk.Messages.Dto.SugarTalk;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ISugarTalkClient : IScopedDependency
{
    Task CloudRecordingCallBackAsync(CloudRecordingCallBackDto request, CancellationToken cancellationToken);
}

public class SugarTalkClient : ISugarTalkClient
{
    private readonly SugarTalkSettings _sugarTalkSettings;
    private readonly Dictionary<string, string> _headers;
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public SugarTalkClient(ISugarTalkHttpClientFactory httpClientFactory, SugarTalkSettings sugarTalkSettings)
    {
        _httpClientFactory = httpClientFactory;
        _sugarTalkSettings = sugarTalkSettings;

        _headers = new Dictionary<string, string>
        {
            { "X-API-KEY", _sugarTalkSettings.ApiKey }
        };
    }

    public async Task CloudRecordingCallBackAsync(CloudRecordingCallBackDto callBackDto, CancellationToken cancellationToken)
    {
        await _httpClientFactory.PostAsJsonAsync<string>(
                $"{_sugarTalkSettings.BaseUrl}/api/Ask/general/query", callBackDto, cancellationToken, headers: _headers).ConfigureAwait(false);
    }
}