using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Foundation;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingDataProvider
{
    Task<List<MeetingProblemFeedbackDto>> GetMeetingProblemFeedbackQueryAsync(GetMeetingProblemFeedbackRequest request, CancellationToken cancellationToken = default);
    
    Task AddMeetingProblemFeedbackAsync(MeetingProblemFeedback feedback, bool forceSave = true, CancellationToken cancellationToken = default);
}

public partial class MeetingDataProvider
{
    public async Task<List<MeetingProblemFeedbackDto>> GetMeetingProblemFeedbackQueryAsync(GetMeetingProblemFeedbackRequest request, CancellationToken cancellationToken = default)
    {
        var feedbackQuery = _repository.Query<MeetingProblemFeedback>();
        var userQuery = _repository.Query<RmStaff>();
        
        var query = from feedback in feedbackQuery
            join user in userQuery on feedback.CreatedBy.ToString() equals user.UserId.ToString()
            select new MeetingProblemFeedbackDto
            {
                FeedbackId = feedback.Id,
                Creator = user.UserName,
                Category = feedback.Category,
                Description = feedback.Description,
                CreatedBy = feedback.CreatedBy,
                CreatedDate = feedback.CreatedDate,
                LastModifiedDate = feedback.LastModifiedDate
            };
        
        if (!string.IsNullOrEmpty(request.KeyWord))
            query = query.Where(x => x.Creator.Contains(request.KeyWord));

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddMeetingProblemFeedbackAsync(MeetingProblemFeedback feedback, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAsync(feedback, cancellationToken).ConfigureAwait(false);
        
        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}