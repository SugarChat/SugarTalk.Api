using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Requests.Meetings;


namespace SugarTalk.Core.Services.Meetings;

public interface IMeetingRecordDetailsService : IScopedDependency
{
    Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(GetMeetingRecordDetailsRequest request, CancellationToken cancellationToken);
}

public class MeetingRecordDetailsService : IMeetingRecordDetailsService
{
    private readonly IMeetingRecordDetailsDataProvider _meetingRecordDetailsDataProvider;
    
    public MeetingRecordDetailsService(IMeetingRecordDetailsDataProvider meetingRecordDetailsDataProvider)
    {
        _meetingRecordDetailsDataProvider = meetingRecordDetailsDataProvider;
    }
    
    public async Task<GetMeetingRecordDetailsResponse> GetMeetingRecordDetailsAsync(GetMeetingRecordDetailsRequest request, CancellationToken cancellationToken)
    {
        var response =  await _meetingRecordDetailsDataProvider.GetMeetingRecordDetailsAsync(request.Id, cancellationToken);
        
        return new GetMeetingRecordDetailsResponse
        {
            Data = response
        };
    }
}