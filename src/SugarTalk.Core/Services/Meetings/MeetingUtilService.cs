using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Dtos.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public interface IMeetingUtilService : IScopedDependency
{
    Task<CreateMeetingResponseDto> CreateMeetingAsync(CreateMeetingDto meeting, CancellationToken cancellationToken);
}

public class MeetingUtilService : IMeetingUtilService
{
    private readonly IAntMediaClient _antMediaClient;

    public MeetingUtilService(IAntMediaClient antMediaClient)
    {
        _antMediaClient = antMediaClient;
    }

    public async Task<CreateMeetingResponseDto> CreateMeetingAsync(CreateMeetingDto meeting, CancellationToken cancellationToken)
    {
        var meetingDto = new CreateMeetingDto
        {
            // MeetingMode = meeting.MeetingMode,
            MeetingNumber = meeting.MeetingNumber,
            RoomStreamList = new List<string>()
        };
        
        return await _antMediaClient.CreateAntMediaConferenceRoomAsync(meetingDto, cancellationToken).ConfigureAwait(false);
    }
}