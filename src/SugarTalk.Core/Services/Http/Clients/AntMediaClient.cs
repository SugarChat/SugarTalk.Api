using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.AntMedia;
using SugarTalk.Messages.Dto.AntMedia;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Core.Services.Http.Clients;

public interface IAntMediaServerClient : IScopedDependency
{
    Task<GetMeetingResponseDto> GetConferenceRoomAsync(
        string appName, string meetingNumber, CancellationToken cancellationToken);

    Task<List<ConferenceRoomDto>> GetConferenceRoomsAsync(
        string appName, int offset, int size, CancellationToken cancellationToken);

    Task<GetConferenceRoomInfoResponseDto> GetConferenceRoomInfoAsync(
        string appName, string roomId, CancellationToken cancellationToken);

    Task<CreateMeetingResponseDto> CreateConferenceRoomAsync(
        string appName, CreateMeetingDto room, CancellationToken cancellationToken);
}

public class AntMediaServerClient : IAntMediaServerClient
{
    private readonly AntMediaServerSetting _antMediaSetting;
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public AntMediaServerClient(AntMediaServerSetting antMediaSetting, ISugarTalkHttpClientFactory httpClientFactory)
    {
        _antMediaSetting = antMediaSetting;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<GetMeetingResponseDto> GetConferenceRoomAsync(
        string appName, string meetingNumber, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<GetMeetingResponseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/{meetingNumber}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<ConferenceRoomDto>> GetConferenceRoomsAsync(
        string appName, int offset, int size, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<List<ConferenceRoomDto>>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/list/{offset}/{size}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetConferenceRoomInfoResponseDto> GetConferenceRoomInfoAsync(
        string appName, string roomId, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<GetConferenceRoomInfoResponseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/{roomId}/room-info", cancellationToken).ConfigureAwait(false);
    }

    public async Task<CreateMeetingResponseDto> CreateConferenceRoomAsync(
        string appName, CreateMeetingDto meetingData, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .PostAsJsonAsync<CreateMeetingResponseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms", meetingData, cancellationToken).ConfigureAwait(false);
    }
}