using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Core.Services.Http.Clients;

namespace SugarTalk.Core.Services.LiveKit;

public interface ILiveKitServerUtilService : IScopedDependency
{
    Task<CreateMeetingFromLiveKitResponseDto> CreateMeetingAsync(
        string meetingNumber, string token, int emptyTimeOut = 600, int maxParticipants = 200, CancellationToken cancellationToken = default);
}

public class LiveKitServerUtilService : ILiveKitServerUtilService
{
    private readonly ILiveKitClient _liveKitClient;

    public LiveKitServerUtilService(ILiveKitClient liveKitClient)
    {
        _liveKitClient = liveKitClient;
    }

    public async Task<CreateMeetingFromLiveKitResponseDto> CreateMeetingAsync(
        string meetingNumber, string token, int emptyTimeOut, int maxParticipants, CancellationToken cancellationToken)
    {
        return new CreateMeetingFromLiveKitResponseDto
        {
            RoomInfo = await _liveKitClient.CreateRoomAsync(token, new CreateLiveKitRoomDto
            {
                MeetingNumber = meetingNumber,
                EmptyTimeOut = emptyTimeOut,
                MaxParticipants = maxParticipants
            }, cancellationToken).ConfigureAwait(false)
        };
    }
}