using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Messages.Dto.Smarties;
using SugarTalk.Core.Settings.Smarties;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ISmartiesClient : IScopedDependency
{
    Task<GetEchoAvatarUserToneResponse> GetEchoAvatarVoiceSettingAsync(GetEchoAvatarVoiceSettingRequestDto request, CancellationToken cancellationToken);
}

public class SmartiesClient : ISmartiesClient
{
    private readonly SmartiesSettings _smartiesSettings;
    private readonly Dictionary<string, string> _headers;
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public SmartiesClient(ISugarTalkHttpClientFactory httpClientFactory, SmartiesSettings smartiesSettings)
    {
        _smartiesSettings = smartiesSettings;
        _httpClientFactory = httpClientFactory;
        
        _headers = new Dictionary<string, string>
        {
            { "X-API-KEY", _smartiesSettings.ApiKey }
        };
    }

    public async Task<GetEchoAvatarUserToneResponse> GetEchoAvatarVoiceSettingAsync(GetEchoAvatarVoiceSettingRequestDto request, CancellationToken cancellationToken)
    {
        return await _httpClientFactory.PostAsJsonAsync<GetEchoAvatarUserToneResponse>(
            $"{_smartiesSettings.BaseUrl}/api/EchoAvatar/voice/setting", request, cancellationToken, headers: _headers).ConfigureAwait(false);
    }
}