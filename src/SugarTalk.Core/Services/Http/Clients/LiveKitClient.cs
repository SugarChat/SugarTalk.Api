using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.LiveKit;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Messages.Dto.LiveKit.Egress;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ILiveKitClient : IScopedDependency
{
    Task<LiveKitRoom> CreateRoomAsync(string token, CreateLiveKitRoomDto room, CancellationToken cancellationToken);

    Task<List<LiveKitRoom>> GetRoomListAsync(string token, List<string> meetingNumbers, CancellationToken cancellationToken);
    
    Task<StartEgressResponseDto> StartRoomCompositeEgressAsync(StartRoomCompositeEgressRequestDto request, CancellationToken cancellationToken);
    
    Task<StartEgressResponseDto> StartTrackCompositeEgressAsync(StartTrackCompositeEgressRequestDto request, CancellationToken cancellationToken);
    
    Task<StartEgressResponseDto> StartTrackEgressAsync(StartTrackEgressRequestDto request, CancellationToken cancellationToken);
    
    Task<StopEgressResponseDto> StopEgressAsync(StopEgressRequestDto request, CancellationToken cancellationToken);
    
    Task<GetEgressInfoListResponseDto> GetEgressInfoListAsync(GetEgressRequestDto request, CancellationToken cancellationToken);
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

    public async Task<List<LiveKitRoom>> GetRoomListAsync(string token, List<string> meetingNumbers, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {token}" }
        };
        
        return await _httpClientFactory
            .PostAsJsonAsync<List<LiveKitRoom>>(
                $"{_liveKitServerSetting.BaseUrl}/twirp/livekit.RoomService/ListRooms", meetingNumbers, cancellationToken, headers: headers).ConfigureAwait(false);
    }

    public async Task<StartEgressResponseDto> StartRoomCompositeEgressAsync(StartRoomCompositeEgressRequestDto request, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {request.Token}" }
        };
        
        return await _httpClientFactory
            .PostAsJsonAsync<StartEgressResponseDto>(
                $"{_liveKitServerSetting.BaseUrl}/twirp/livekit.Egress/StartRoomCompositeEgress", request, cancellationToken, headers: headers).ConfigureAwait(false);
    }

    public async Task<StartEgressResponseDto> StartTrackCompositeEgressAsync(StartTrackCompositeEgressRequestDto request, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {request.Token}" }
        };
        
        return await _httpClientFactory
            .PostAsJsonAsync<StartEgressResponseDto>(
                $"{_liveKitServerSetting.BaseUrl}/twirp/livekit.Egress/StartTrackCompositeEgress", request, cancellationToken, headers: headers).ConfigureAwait(false);
    }

    public async Task<StartEgressResponseDto> StartTrackEgressAsync(StartTrackEgressRequestDto request, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {request.Token}" }
        };
        
        return await _httpClientFactory
            .PostAsJsonAsync<StartEgressResponseDto>(
                $"{_liveKitServerSetting.BaseUrl}/twirp/livekit.Egress/StartTrackEgress", request, cancellationToken, headers: headers).ConfigureAwait(false);
    }

    public async Task<StopEgressResponseDto> StopEgressAsync(StopEgressRequestDto request, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {request.Token}" }
        };
        
        return await _httpClientFactory
            .PostAsJsonAsync<StopEgressResponseDto>($"{_liveKitServerSetting.BaseUrl}/twirp/livekit.Egress/StopEgress", request, cancellationToken, headers: headers).ConfigureAwait(false);
    }

    public async Task<GetEgressInfoListResponseDto> GetEgressInfoListAsync(GetEgressRequestDto request, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {request.Token}" }
        };
        
        return await _httpClientFactory
            .PostAsJsonAsync<GetEgressInfoListResponseDto>($"{_liveKitServerSetting.BaseUrl}/twirp/livekit.Egress/ListEgress", request, cancellationToken, headers: headers).ConfigureAwait(false);
    }
}