using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.LiveKit;
using SugarTalk.Messages.Dto.LiveKit;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ILiveKitClient : IScopedDependency
{
    Task<LiveKitRoom> CreateRoomAsync(string token, CreateLiveKitRoomDto room, CancellationToken cancellationToken);

    Task<List<LiveKitRoom>> GetRoomListAsync(string token, List<string> MeetingNumbers, CancellationToken cancellationToken);
}

public class LiveKitClient : ILiveKitClient
{
    private readonly LiveKitServerSetting _liveKitServerSetting;
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public LiveKitClient(LiveKitServerSetting liveKitServerSetting, ISugarTalkHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _liveKitServerSetting = liveKitServerSetting;
    }

    public async Task<LiveKitRoom> CreateRoomAsync(string token, CreateLiveKitRoomDto room, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {token}" }
        };

        return await _httpClientFactory
            .PostAsJsonAsync<LiveKitRoom>(
                $"{_liveKitServerSetting.BaseUrl}/twirp/livekit.RoomService/CreateRoom", room, cancellationToken, headers: headers).ConfigureAwait(false);
    }

    public async Task<List<LiveKitRoom>> GetRoomListAsync(string token, List<string> MeetingNumbers, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {token}" }
        };
        
        return await _httpClientFactory
            .PostAsJsonAsync<List<LiveKitRoom>>(
                $"{_liveKitServerSetting.BaseUrl}/twirp/livekit.RoomService/ListRooms", MeetingNumbers, cancellationToken, headers: headers).ConfigureAwait(false);
    }
}