using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SugarTalk.Core.Domain.Account.Exceptions;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<GetMeetingProblemFeedbackResponse> GetMeetingProblemFeedbackAsync(GetMeetingProblemFeedbackRequest request, CancellationToken cancellationToken);
    
    Task<MeetingProblemFeedbackAddedEvent> AddMeetingProblemFeedbackAsync(AddMeetingProblemFeedbackCommand command, CancellationToken cancellationToken);
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

    public async Task<MeetingProblemFeedbackAddedEvent> AddMeetingProblemFeedbackAsync(AddMeetingProblemFeedbackCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.Id is null) throw new UserAccountNotFoundException();
        
        var feedback = _mapper.Map<MeetingProblemFeedback>(command.Feedback);
        feedback.LastModifiedDate = DateTimeOffset.Now;
        
        await _meetingDataProvider.AddMeetingProblemFeedbackAsync(feedback, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        Log.Information("Add IdentifyFile FeedBack Async feedback: {@feedback}", feedback);
        
        return new MeetingProblemFeedbackAddedEvent
        {
            Feedback = _mapper.Map<MeetingProblemFeedbackDto>(feedback)
        };
    }
}