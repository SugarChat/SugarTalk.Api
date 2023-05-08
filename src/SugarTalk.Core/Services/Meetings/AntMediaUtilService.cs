using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public interface IAntMediaUtilService : IScopedDependency
{
    Task<CreateMeetingResponseDto> CreateMeetingAsync(
        CreateMeetingDto meeting, string appName, CancellationToken cancellationToken);
    
    Task<GetMeetingResponseDto> GetMeetingByMeetingNumberAsync(
        string meetingNumber, string appName, CancellationToken cancellationToken);
}

public class AntMediaUtilService : IAntMediaUtilService
{
    private readonly AntMediaServerClient _antMediaServerClient;

    public AntMediaUtilService(AntMediaServerClient antMediaServerClient)
    {
        _antMediaServerClient = antMediaServerClient;
    }

    public async Task<CreateMeetingResponseDto> CreateMeetingAsync(CreateMeetingDto meeting, string appName, CancellationToken cancellationToken)
    {
        return await _antMediaServerClient.CreateAntMediaConferenceRoomAsync(meeting, appName, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetMeetingResponseDto> GetMeetingByMeetingNumberAsync(string meetingNumber, string appName, CancellationToken cancellationToken)
    {
        return await _antMediaServerClient.GetAntMediaConferenceRoomAsync(meetingNumber, appName, cancellationToken).ConfigureAwait(false);
    }
}