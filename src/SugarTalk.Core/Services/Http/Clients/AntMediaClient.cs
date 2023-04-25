using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.AntMedia;
using SugarTalk.Messages.Dtos.AntMedia;
using SugarTalk.Messages.Dtos.Meetings;

namespace SugarTalk.Core.Services.Http.Clients;

public interface IAntMediaClient : IScopedDependency
{
    Task<ConferenceRoomDto> GetAntMediaConferenceRoomAsync(
        string roomId, CancellationToken cancellationToken);

    Task<List<ConferenceRoomDto>> GetAntMediaConferenceRoomsAsync(
        int offset, int size, CancellationToken cancellationToken);

    Task<GetAntMediaConferenceRoomInfoResponseDto> GetAntMediaConferenceRoomInfoAsync(
        string roomId, CancellationToken cancellationToken);

    Task<List<AntMediaBroadcastDto>> GetAntMediaBroadcastsAsync(
        int offset, int size, CancellationToken cancellationToken);

    Task<CreateMeetingResponseDto> CreateAntMediaConferenceRoomAsync(
        CreateMeetingDto room, CancellationToken cancellationToken);
}

public class AntMediaClient : IAntMediaClient
{
    private readonly AntMediaSetting _antMediaSetting;
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public AntMediaClient(AntMediaSetting antMediaSetting, ISugarTalkHttpClientFactory httpClientFactory)
    {
        _antMediaSetting = antMediaSetting;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConferenceRoomDto> GetAntMediaConferenceRoomAsync(string roomId, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<ConferenceRoomDto>(
                $"{_antMediaSetting.BroadcastUrl}/conference-rooms/{roomId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<ConferenceRoomDto>> GetAntMediaConferenceRoomsAsync(int offset, int size,
        CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<List<ConferenceRoomDto>>(
                $"{_antMediaSetting.BroadcastUrl}/conference-rooms/list/{offset}/{size}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetAntMediaConferenceRoomInfoResponseDto> GetAntMediaConferenceRoomInfoAsync(string roomId, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<GetAntMediaConferenceRoomInfoResponseDto>(
                $"{_antMediaSetting.BroadcastUrl}/conference-rooms/{roomId}/room-info", cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<AntMediaBroadcastDto>> GetAntMediaBroadcastsAsync(int offset, int size, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<List<AntMediaBroadcastDto>>(
                $"{_antMediaSetting.BroadcastUrl}/list/{offset}/{size}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<CreateMeetingResponseDto> CreateAntMediaConferenceRoomAsync(CreateMeetingDto meetingData, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .PostAsJsonAsync<CreateMeetingResponseDto>(
                $"{_antMediaSetting.BroadcastUrl}/conference-rooms", meetingData, cancellationToken).ConfigureAwait(false);
    }
}