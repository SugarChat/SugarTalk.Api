using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Foundation;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<(List<MeetingProblemFeedbackDto>, int)> GetMeetingProblemFeedbacksAsync(GetMeetingProblemFeedbackRequest request, CancellationToken cancellationToken = default);
    
    Task AddMeetingProblemFeedbackAsync(MeetingProblemFeedback feedback, bool forceSave = true, CancellationToken cancellationToken = default);
}

public partial class MeetingDataProvider
{
    public async Task<(List<MeetingProblemFeedbackDto>, int)> GetMeetingProblemFeedbacksAsync(GetMeetingProblemFeedbackRequest request, CancellationToken cancellationToken = default)
    {
        var feedbackQuery = _repository.Query<MeetingProblemFeedback>();
        var userQuery = _repository.Query<UserAccount>();
        
        var query = from feedback in feedbackQuery
            join user in userQuery on feedback.CreatedBy equals user.Id
            select new MeetingProblemFeedbackDto
            {
                Creator = user.UserName,
                Category = feedback.Category,
                Description = feedback.Description
            };
        
        if (!string.IsNullOrEmpty(request.KeyWord))
            query = query.Where(x => x.Creator.Contains(request.KeyWord));
        
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