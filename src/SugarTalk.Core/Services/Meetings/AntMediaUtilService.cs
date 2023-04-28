using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public interface IAntMediaUtilService : IScopedDependency
{
    Task<CreateMeetingResponseDto> CreateMeetingAsync(
        CreateMeetingDto meeting, CancellationToken cancellationToken);
    
    Task<GetMeetingResponseDto> GetMeetingByMeetingNumberAsync(
        string meetingNumber, CancellationToken cancellationToken);
}

public class AntMediaUtilService : IAntMediaUtilService
{
    private readonly IAntMediaClient _antMediaClient;

    public AntMediaUtilService(IAntMediaClient antMediaClient)
    {
        _antMediaClient = antMediaClient;
    }

    public async Task<CreateMeetingResponseDto> CreateMeetingAsync(CreateMeetingDto meeting, CancellationToken cancellationToken)
    {
        return await _antMediaClient.CreateAntMediaConferenceRoomAsync(meeting, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetMeetingResponseDto> GetMeetingByMeetingNumberAsync(string meetingNumber, CancellationToken cancellationToken)
    {
        return await _antMediaClient.GetAntMediaConferenceRoomAsync(meetingNumber, cancellationToken).ConfigureAwait(false);
    }
}