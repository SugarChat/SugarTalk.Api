using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.SuagrTalk;
using SugarTalk.Messages.Commands.Tencent;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ISugarTalkClient : IScopedDependency
{
    Task CloudRecordingCallBackAsync(CloudRecordingCallBackCommand callBackCommand, CancellationToken cancellationToken);
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

    public async Task CloudRecordingCallBackAsync(CloudRecordingCallBackCommand callBackCommand, CancellationToken cancellationToken)
    {
        Log.Information("SugarTalkClient.CloudRecordingCallBackCommand: {@callBackCommand}", callBackCommand);
        
        var response = await _httpClientFactory
            .PostAsJsonAsync<string>($"{_sugarTalkSettings.BaseUrl}/api/Tencent/cloudRecord/callback", callBackCommand, cancellationToken, headers: _headers).ConfigureAwait(false);
        
        Log.Information("SugarTalkClient.CloudRecordingCallBackCommand: {@response}", response);
    }
}