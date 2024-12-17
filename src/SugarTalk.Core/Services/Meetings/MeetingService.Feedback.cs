using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<GetMeetingProblemFeedbackResponse> GetMeetingProblemFeedbackAsync(GetMeetingProblemFeedbackRequest request, CancellationToken cancellationToken);
    
    Task AddMeetingProblemFeedbackAsync(AddMeetingProblemFeedbackCommand command, CancellationToken cancellationToken);
}

public partial class MeetingService
{
    public async Task<GetMeetingProblemFeedbackResponse> GetMeetingProblemFeedbackAsync(GetMeetingProblemFeedbackRequest request, CancellationToken cancellationToken)
    {
        var (feedbackList, totalCount) = await _meetingDataProvider.GetMeetingProblemFeedbacksAsync(request, cancellationToken);
        
        return new GetMeetingProblemFeedbackResponse
        {
            FeedbackDto = feedbackList,
            Count = totalCount
        };
    }

    public async Task AddMeetingProblemFeedbackAsync(AddMeetingProblemFeedbackCommand command, CancellationToken cancellationToken)
    {
        var feedback = _mapper.Map<MeetingProblemFeedback>(command.Feedback);
        
        await _meetingDataProvider.AddMeetingProblemFeedbackAsync(feedback, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        Log.Information("Add IdentifyFile FeedBack Async feedback: {@feedback}", feedback);
    }
}