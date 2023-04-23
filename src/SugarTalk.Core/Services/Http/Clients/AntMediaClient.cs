using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.AntMedia;
using SugarTalk.Messages.Dtos.AntMedia;

namespace SugarTalk.Core.Services.Http.Clients;

public interface IAntMediaClient : IScopedDependency
{
    Task<GetAntMediaConferenceRoomResponseDto> GetAntMediaConferenceRoomAsync(
        string roomId, CancellationToken cancellationToken);

    Task<GetAntMediaConferenceRoomsResponseDto> GetAntMediaConferenceRoomsAsync(
        int offset, int size, CancellationToken cancellationToken);

    Task<GetAntMediaConferenceRoomInfoResponseDto> GetAntMediaConferenceRoomInfoAsync(
        string roomId, CancellationToken cancellationToken);

    Task<List<GetAntMediaBroadcastResponseData>> GetAntMediaBroadcastsAsync(
        int offset, int size, CancellationToken cancellationToken);

    Task<GetAntMediaConferenceRoomResponseDto> CreateAntMediaConferenceRoomAsync(
        GetAntMediaConferenceRoomResponseDto room, CancellationToken cancellationToken);
}

// https://talk.sjdistributors.com:5443/LiveApp/rest/v2/broadcasts/conference-rooms/sj-room/room-info
public class AntMediaClient : IAntMediaClient
{
    private readonly AntMediaSetting _antMediaSetting;
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public AntMediaClient(AntMediaSetting antMediaSetting, ISugarTalkHttpClientFactory httpClientFactory)
    {
        _antMediaSetting = antMediaSetting;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<GetAntMediaConferenceRoomResponseDto> GetAntMediaConferenceRoomAsync(string roomId, CancellationToken cancellationToken)
    {
        // var broadcastUrl = $"https://talk.sjdistributors.com:5443/LiveApp/rest/v2/broadcasts/conference-rooms/{roomId}";

        return await _httpClientFactory
            .GetAsync<GetAntMediaConferenceRoomResponseDto>(
                $"{_antMediaSetting.BroadcastUrl}/conference-rooms/{roomId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetAntMediaConferenceRoomsResponseDto> GetAntMediaConferenceRoomsAsync(int offset, int size,
        CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<GetAntMediaConferenceRoomsResponseDto>(
                $"{_antMediaSetting.BroadcastUrl}/conference-rooms/list/{offset}/{size}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetAntMediaConferenceRoomInfoResponseDto> GetAntMediaConferenceRoomInfoAsync(string roomId, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<GetAntMediaConferenceRoomInfoResponseDto>(
                $"{_antMediaSetting.BroadcastUrl}/conference-rooms/{roomId}/room-info", cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<GetAntMediaBroadcastResponseData>> GetAntMediaBroadcastsAsync(int offset, int size, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<List<GetAntMediaBroadcastResponseData>>(
                $"{_antMediaSetting.BroadcastUrl}/list/{offset}/{size}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetAntMediaConferenceRoomResponseDto> CreateAntMediaConferenceRoomAsync(GetAntMediaConferenceRoomResponseDto roomData, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .PostAsJsonAsync<GetAntMediaConferenceRoomResponseDto>(
                $"{_antMediaSetting.BroadcastUrl}/conference-rooms", roomData, cancellationToken).ConfigureAwait(false);
    }
}