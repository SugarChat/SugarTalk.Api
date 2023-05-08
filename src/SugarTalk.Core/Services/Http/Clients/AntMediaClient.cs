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
    Task<GetMeetingResponseDto> GetAntMediaConferenceRoomAsync(
        string meetingNumber, string appName, CancellationToken cancellationToken);

    Task<List<ConferenceRoomDto>> GetAntMediaConferenceRoomsAsync(
        int offset, int size, string appName, CancellationToken cancellationToken);

    Task<GetAntMediaConferenceRoomInfoResponseDto> GetAntMediaConferenceRoomInfoAsync(
        string roomId, string appName, CancellationToken cancellationToken);

    Task<CreateMeetingResponseDto> CreateAntMediaConferenceRoomAsync(
        CreateMeetingDto room, string appName, CancellationToken cancellationToken);
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

    public async Task<GetMeetingResponseDto> GetAntMediaConferenceRoomAsync(
        string meetingNumber, string appName, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<GetMeetingResponseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/{meetingNumber}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<ConferenceRoomDto>> GetAntMediaConferenceRoomsAsync(
        int offset, int size, string appName, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<List<ConferenceRoomDto>>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/list/{offset}/{size}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetAntMediaConferenceRoomInfoResponseDto> GetAntMediaConferenceRoomInfoAsync(
        string roomId, string appName, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .GetAsync<GetAntMediaConferenceRoomInfoResponseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/{roomId}/room-info", cancellationToken).ConfigureAwait(false);
    }

    public async Task<CreateMeetingResponseDto> CreateAntMediaConferenceRoomAsync(
        CreateMeetingDto meetingData, string appName, CancellationToken cancellationToken)
    {
        return await _httpClientFactory
            .PostAsJsonAsync<CreateMeetingResponseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms", meetingData, cancellationToken).ConfigureAwait(false);
    }
}