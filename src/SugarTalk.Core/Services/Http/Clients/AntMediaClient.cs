using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
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
    
    Task<ConferenceRoomResponseBaseDto> AddStreamToConferenceRoomAsync(
        string appName, string meetingNumber, string streamId, CancellationToken cancellationToken);

    Task<ConferenceRoomResponseBaseDto> DeleteConferenceRoomAsync(
        string appName, string meetingNumber, CancellationToken cancellationToken);
    
    Task<ConferenceRoomResponseBaseDto> DeleteStreamFromConferenceRoomAsync(
        string appName, string meetingNumber, string streamId, CancellationToken cancellationToken);
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
        var response = await _httpClientFactory
            .GetAsync<GetMeetingResponseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/{meetingNumber}", cancellationToken).ConfigureAwait(false);

        Log.Information("Ant Media to get conference room, response: {response}", response);
        
        return response;
    }

    public async Task<List<ConferenceRoomDto>> GetConferenceRoomsAsync(
        string appName, int offset, int size, CancellationToken cancellationToken)
    {
        var response = await _httpClientFactory
            .GetAsync<List<ConferenceRoomDto>>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/list/{offset}/{size}", cancellationToken).ConfigureAwait(false);
        
        Log.Information("Ant Media to get all conference room, response: {response}", response);

        return response;
    }

    public async Task<GetConferenceRoomInfoResponseDto> GetConferenceRoomInfoAsync(
        string appName, string roomId, CancellationToken cancellationToken)
    {
        var response = await _httpClientFactory
            .GetAsync<GetConferenceRoomInfoResponseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/{roomId}/room-info", cancellationToken).ConfigureAwait(false);
    
        Log.Information("Ant Media to get conference room info, response: {response}", response);
        
        return response;
    }

    public async Task<CreateMeetingResponseDto> CreateConferenceRoomAsync(
        string appName, CreateMeetingDto meetingData, CancellationToken cancellationToken)
    {
        var response = await _httpClientFactory
            .PostAsJsonAsync<CreateMeetingResponseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms", meetingData, cancellationToken).ConfigureAwait(false);
        
        Log.Information("Ant Media to create conference room, response: {response}", response);
        
        return response;
    }

    public async Task<ConferenceRoomResponseBaseDto> DeleteConferenceRoomAsync(
        string appName, string meetingNumber, CancellationToken cancellationToken)
    {
        var response = await _httpClientFactory
            .DeleteAsync<ConferenceRoomResponseBaseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/{meetingNumber}", cancellationToken).ConfigureAwait(false);
        
        Log.Information("Ant Media to delete conference room, response: {response}", response);
        
        return response;
    }

    public async Task<ConferenceRoomResponseBaseDto> AddStreamToConferenceRoomAsync(
        string appName, string meetingNumber, string streamId, CancellationToken cancellationToken)
    {
        var response = await _httpClientFactory
            .PutAsync<ConferenceRoomResponseBaseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/{meetingNumber}/add?streamId={streamId}", null, cancellationToken).ConfigureAwait(false);
        
        Log.Information("Ant Media add stream to conference room , response: {response}", response);
        
        return response;
    }
    
    public async Task<ConferenceRoomResponseBaseDto> DeleteStreamFromConferenceRoomAsync(
        string appName, string meetingNumber, string streamId, CancellationToken cancellationToken)
    {
        var response = await _httpClientFactory
            .PutAsync<ConferenceRoomResponseBaseDto>(
                $"{_antMediaSetting.BaseUrl}/{appName}/rest/v2/broadcasts/conference-rooms/{meetingNumber}/delete?streamId={streamId}", null, cancellationToken).ConfigureAwait(false);
        
        Log.Information("Ant Media delete stream from conference room, response: {response}", response);
        
        return response;
    }
}