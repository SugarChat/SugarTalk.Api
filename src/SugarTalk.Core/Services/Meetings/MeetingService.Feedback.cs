using Serilog;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Caching;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings.Feedback;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    Task<GetMeetingProblemFeedbackResponse> GetMeetingProblemFeedbackAsync(GetMeetingProblemFeedbackRequest request, CancellationToken cancellationToken);
    
    Task<AddMeetingProblemFeedbackResponse> AddMeetingProblemFeedbackAsync(AddMeetingProblemFeedbackCommand command, CancellationToken cancellationToken);

    Task<GetMeetingProblemFeedbacksCountResponse> GetMeetingProblemFeedbacksCountAsync(GetMeetingProblemFeedbacksCountRequest request, CancellationToken cancellationToken);

    Task<UpdateMeetingProblemFeedbackUnreadCountResponse> UpdateMeetingProblemFeedbackUnreadCountAsync(UpdateMeetingProblemFeedbackUnreadCountCommand command, CancellationToken cancellationToken);
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

    public async Task<AddMeetingProblemFeedbackResponse> AddMeetingProblemFeedbackAsync(AddMeetingProblemFeedbackCommand command, CancellationToken cancellationToken)
    {
        var feedback = _mapper.Map<MeetingProblemFeedback>(command.Feedback);
        
        await _meetingDataProvider.AddMeetingProblemFeedbackAsync(feedback, cancellationToken: cancellationToken).ConfigureAwait(false);

        _backgroundJobClient.Enqueue(() => AddFeedbackCountForAccountAsync(_feedbackSettings.AccountName, cancellationToken));
        
        Log.Information("Add IdentifyFile FeedBack Async feedback: {@feedback}", feedback);

        return new AddMeetingProblemFeedbackResponse();
    }

    public async Task AddFeedbackCountForAccountAsync(List<string> userNames, CancellationToken cancellationToken)
    {
        foreach (var accountName in userNames)
        {
            var feedbackCount = await _cacheManager.GetOrAddAsync(accountName, () => Task.FromResult(new FeedbackCountDto(accountName)), CachingType.RedisCache, cancellationToken: cancellationToken).ConfigureAwait(false);

            feedbackCount.Count += 1;
            
            await _cacheManager.SetAsync(accountName, feedbackCount, CachingType.RedisCache, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<GetMeetingProblemFeedbacksCountResponse> GetMeetingProblemFeedbacksCountAsync(GetMeetingProblemFeedbacksCountRequest request, CancellationToken cancellationToken)
    {
        if (_feedbackSettings.AccountName.Where(x => x == _currentUser.Name).ToList() is { Count: <= 0 })
            return new GetMeetingProblemFeedbacksCountResponse
            {
                Code = HttpStatusCode.Unauthorized,
                Msg = "User Unauthorized"
            };
        
        var count = await _cacheManager.GetOrAddAsync(
            _currentUser.Name, () => Task.FromResult(new FeedbackCountDto(_currentUser.Name)), CachingType.RedisCache, cancellationToken: cancellationToken).ConfigureAwait(false);

        return new GetMeetingProblemFeedbacksCountResponse
        {
            Data = count.Count
        };
    }

    public async Task<UpdateMeetingProblemFeedbackUnreadCountResponse> UpdateMeetingProblemFeedbackUnreadCountAsync(UpdateMeetingProblemFeedbackUnreadCountCommand command, CancellationToken cancellationToken)
    {
        await _cacheManager.SetAsync(_currentUser.Name, new FeedbackCountDto(_currentUser.Name), CachingType.RedisCache, cancellationToken: cancellationToken).ConfigureAwait(false);

        return new UpdateMeetingProblemFeedbackUnreadCountResponse();
    }
}