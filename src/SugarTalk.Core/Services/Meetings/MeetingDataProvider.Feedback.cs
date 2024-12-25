using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<(List<GetMeetingProblemFeedbackDto>, int)> GetMeetingProblemFeedbacksAsync(GetMeetingProblemFeedbackRequest request, CancellationToken cancellationToken);
    
    Task AddMeetingProblemFeedbackAsync(MeetingProblemFeedback feedback, bool forceSave = true, CancellationToken cancellationToken = default);
}

public partial class MeetingDataProvider
{
    public async Task<(List<GetMeetingProblemFeedbackDto>, int)> GetMeetingProblemFeedbacksAsync(GetMeetingProblemFeedbackRequest request, CancellationToken cancellationToken)
    {
        var query = from feedback in _repository.Query<MeetingProblemFeedback>()
            join user in _repository.Query<UserAccount>() on feedback.CreatedBy equals user.Id
            where string.IsNullOrEmpty(request.KeyWord) || user.UserName.Contains(request.KeyWord)
            select new GetMeetingProblemFeedbackDto
            {
                FeedbackId = feedback.Id,
                Creator = user.UserName,
                Categories = JsonConvert.DeserializeObject<List<MeetingCategoryType>>(feedback.Categories),
                Description = feedback.Description,
                LastModifiedDate = feedback.LastModifiedDate
            };
        
        var totalCount = await query.CountAsync(cancellationToken);

        var feedbackList = await query.Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

        return (feedbackList, totalCount);
    }

    public async Task AddMeetingProblemFeedbackAsync(MeetingProblemFeedback feedback, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAsync(feedback, cancellationToken).ConfigureAwait(false);
        
        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}