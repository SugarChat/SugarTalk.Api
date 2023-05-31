using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Core.Services.AntMediaServer;

public interface IAntMediaServerUtilService : IScopedDependency
{
    Task<CreateMeetingResponseDto> CreateMeetingAsync(
        string appName, CreateMeetingDto meeting, CancellationToken cancellationToken);
    
    Task<GetMeetingResponseDto> GetMeetingByMeetingNumberAsync(
        string appName, string meetingNumber, CancellationToken cancellationToken);

    Task<ConferenceRoomResponseBaseDto> RemoveMeetingByMeetingNumberAsync(
        string appName, string meetingNumber, CancellationToken cancellationToken);

    Task<ConferenceRoomResponseBaseDto> AddStreamToMeetingAsync(
        string appName, string meetingNumber, string streamId, CancellationToken cancellationToken);
    
    Task<ConferenceRoomResponseBaseDto> RemoveStreamFromMeetingAsync(
        string appName, string meetingNumber, string streamId, CancellationToken cancellationToken);

    Task<CreateMeetingStreamResponseDto> CreateCustomStreamAsync(
        string appName, CreateMeetingStreamDto createMeetingStream, CancellationToken cancellationToken, bool autoStart = false);
}

public class AntMediaServerUtilService : IAntMediaServerUtilService
{
    private readonly IAntMediaServerClient _antMediaServerClient;

    public AntMediaServerUtilService(IAntMediaServerClient antMediaServerClient)
    {
        _antMediaServerClient = antMediaServerClient;
    }

    public async Task<CreateMeetingResponseDto> CreateMeetingAsync(string appName, CreateMeetingDto meeting, CancellationToken cancellationToken)
    {
        return await _antMediaServerClient.CreateConferenceRoomAsync(appName, meeting, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetMeetingResponseDto> GetMeetingByMeetingNumberAsync(string appName, string meetingNumber, CancellationToken cancellationToken)
    {
        return await _antMediaServerClient.GetConferenceRoomAsync(appName, meetingNumber, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ConferenceRoomResponseBaseDto> RemoveMeetingByMeetingNumberAsync(string appName, string meetingNumber, CancellationToken cancellationToken)
    {
        return await _antMediaServerClient.DeleteConferenceRoomAsync(appName, meetingNumber, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ConferenceRoomResponseBaseDto> AddStreamToMeetingAsync(string appName, string meetingNumber, string streamId, CancellationToken cancellationToken)
    {
        return await _antMediaServerClient
            .AddStreamToConferenceRoomAsync(appName, meetingNumber, streamId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ConferenceRoomResponseBaseDto> RemoveStreamFromMeetingAsync(string appName, string meetingNumber, string streamId, CancellationToken cancellationToken)
    {
        return await _antMediaServerClient
            .DeleteStreamFromConferenceRoomAsync(appName, meetingNumber, streamId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CreateMeetingStreamResponseDto> CreateCustomStreamAsync(
        string appName, CreateMeetingStreamDto createMeetingStream, CancellationToken cancellationToken, bool autoStart = false)
    {
        return await _antMediaServerClient
            .CreateCustomStreamAsync(appName, autoStart, createMeetingStream, cancellationToken).ConfigureAwait(false);
    }
}