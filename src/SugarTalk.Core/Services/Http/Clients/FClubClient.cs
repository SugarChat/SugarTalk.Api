using Serilog;
using Newtonsoft.Json;
using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Messages.Dto.FClub;
using SugarTalk.Core.Settings.FClub;

namespace SugarTalk.Core.Services.Http.Clients;

public interface IFClubClient : IScopedDependency
{
    Task<CombineMp4VideosResponse> CombineMp4VideosAsync(CombineMp4VideosDto request, CancellationToken cancellationToken);
    
    Task<CombineMp4VideosTaskResponse> CombineMp4VideosTaskAsync(CombineMp4VideosTaskDto request, CancellationToken cancellationToken);
    
    Task<GetCombineMp4VideosTaskResponse> GetCombineMp4VideoTaskAsync(GetCombineMp4VideoTaskByIdDto request, CancellationToken cancellationToken);
}

public class FClubClient : IFClubClient
{
    private readonly FClubSetting _fClubSetting;
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public FClubClient(FClubSetting fClubSetting, ISugarTalkHttpClientFactory httpClientFactory)
    {
        _fClubSetting = fClubSetting;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CombineMp4VideosResponse> CombineMp4VideosAsync(CombineMp4VideosDto request, CancellationToken cancellationToken)
    {
        var header = new Dictionary<string, string>
        {
            {"X-API-KEY", _fClubSetting.ApiKey}
        };
        
        return await _httpClientFactory.PostAsJsonAsync<CombineMp4VideosResponse>(
            $"{_fClubSetting.BaseUrl}/Combine", request, cancellationToken, headers: header).ConfigureAwait(false);
    }

    public async Task<CombineMp4VideosTaskResponse> CombineMp4VideosTaskAsync(CombineMp4VideosTaskDto request, CancellationToken cancellationToken)
    {
        var header = new Dictionary<string, string>
        {
            {"X-API-KEY", _fClubSetting.ApiKey}
        };
        
        return await _httpClientFactory.PostAsJsonAsync<CombineMp4VideosTaskResponse>(
            $"{_fClubSetting.BaseUrl}/Combine/task", request, cancellationToken, headers: header).ConfigureAwait(false);
    }

    public async Task<GetCombineMp4VideosTaskResponse> GetCombineMp4VideoTaskAsync(
        GetCombineMp4VideoTaskByIdDto request, CancellationToken cancellationToken)
    {
        var header = new Dictionary<string, string>
        {
            {"X-API-KEY", _fClubSetting.ApiKey}
        };
        
        var response = await _httpClientFactory
            .GetAsync<GetCombineMp4VideosTaskResponse>(
                $"{_fClubSetting.BaseUrl}/Combine/task/{request.TaskId}", cancellationToken, headers: header).ConfigureAwait(false);
        
        Log.Information("response: {response}", JsonConvert.SerializeObject(response));

        return response;
    }
}