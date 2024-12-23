using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Fact] 
    public async Task CanGetMeetingProblemFeedbackAsync() 
    { 
        var testUser = new UserAccount { Id = 9999, UserName = "TestUser", Password = "TestPassword"}; 
        
        var testFeedback1 = new MeetingProblemFeedback 
        { 
            Id = 1001, 
            CreatedBy = testUser.Id, 
            Categories  = (int)MeetingCategoryType.Suggestions,
            Description = "Test Feedback 1", 
            LastModifiedDate = DateTimeOffset.Now 
        }; 
        var testFeedback2 = new MeetingProblemFeedback 
        { 
            Id = 1002, 
            CreatedBy = testUser.Id, 
            Categories = (int)MeetingCategoryType.Defect,
            Description = "Test Feedback 2", 
            LastModifiedDate = DateTimeOffset.Now 
        }; 
        
        await RunWithUnitOfWork<IRepository>(async (repository) => 
        { 
            await repository.InsertAsync(testUser);
            await repository.InsertAsync(testFeedback1); 
            await repository.InsertAsync(testFeedback2);
        });

        await RunWithUnitOfWork<IMediator>(async mediator =>
        {
            var request = new GetMeetingProblemFeedbackRequest
            {
                KeyWord = "TestUser"
            };

            var response = await mediator
                .RequestAsync<GetMeetingProblemFeedbackRequest, GetMeetingProblemFeedbackResponse>(request)
                .ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.Count.ShouldBe(2);
            response.Data.FeedbackDto.Count.ShouldBe(2);
            
            var feedback1 = response.Data.FeedbackDto.FirstOrDefault(f => f.FeedbackId == testFeedback1.Id);
            feedback1.ShouldNotBeNull();
            feedback1.Creator.ShouldBe(testUser.UserName);
            feedback1.Description.ShouldBe(testFeedback1.Description);
            
            var feedback2 = response.Data.FeedbackDto.FirstOrDefault(f => f.FeedbackId == testFeedback2.Id);
            feedback2.ShouldNotBeNull();
            feedback2.Creator.ShouldBe(testUser.UserName);
            feedback2.Description.ShouldBe(testFeedback2.Description);
        });
    }
    
    [Fact]
    public async Task CanAddMeetingProblemFeedbackAsync()
    {
        var testFeedbackDto = new MeetingProblemFeedbackDto
        {
            Categories = new List<MeetingCategoryType>
            {
               MeetingCategoryType.Suggestions,MeetingCategoryType.Defect
            },
            Description = "Test Feedback"
        };

        var command = new AddMeetingProblemFeedbackCommand
        {
            Feedback = testFeedbackDto
        };

        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await RunWithUnitOfWork<IMediator>(async mediator =>
            {
                await mediator.SendAsync(command).ConfigureAwait(false);
            });
            
            var feedbackFromDb = await repository
                .FirstOrDefaultAsync<MeetingProblemFeedback>(f => f.Description == testFeedbackDto.Description)
                .ConfigureAwait(false);
            
            feedbackFromDb.ShouldNotBeNull();
            feedbackFromDb.Categories.ShouldBe((int)MeetingCategoryType.Suggestions | (int)MeetingCategoryType.Defect);
            feedbackFromDb.Description.ShouldBe(testFeedbackDto.Description);
            feedbackFromDb.LastModifiedDate.ShouldBeGreaterThan(DateTimeOffset.Now.AddMinutes(-1));
        });
    }
}