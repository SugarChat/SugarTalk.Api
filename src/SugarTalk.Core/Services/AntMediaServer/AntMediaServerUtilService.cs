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
        return await _antMediaServerClient.CreateAntMediaConferenceRoomAsync(appName, meeting, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetMeetingResponseDto> GetMeetingByMeetingNumberAsync(string appName, string meetingNumber, CancellationToken cancellationToken)
    {
        return await _antMediaServerClient.GetAntMediaConferenceRoomAsync(appName, meetingNumber, cancellationToken).ConfigureAwait(false);
    }
}