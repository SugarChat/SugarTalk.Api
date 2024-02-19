using System.Threading.Tasks;
using Autofac;
using Mediator.Net;
using NSubstitute;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Middlewares.GuestValidator;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.IntegrationTests.TestBaseClasses;
using SugarTalk.IntegrationTests.Utils.Account;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Enums.Account;
using Xunit;

namespace SugarTalk.IntegrationTests.Middleware.GuestValidator;

public class GuestValidatorMiddlewareTests : GenericFixtureBase
{
    [Fact]
    public async Task ShouldGuestJoinMeetingWhenAllow()
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            var currentUser = await repository.GetByIdAsync<UserAccount>(1);
            
            currentUser.Issuer = UserAccountIssuer.Guest;
            
            await repository.UpdateAsync(currentUser);
            
            await repository.InsertAsync(new Meeting
            {
                MeetingNumber = "123"
            });
        });
        
        await RunWithUnitOfWork<IMediator>(async mediator =>
        {   
            await mediator.SendAsync(new JoinMeetingCommand
            {
                MeetingNumber = "123",
                SecurityCode = "1",
                IsMuted = false
            });
        }, builder =>
        {
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();
            
            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>()).Returns("");
            
            builder.RegisterInstance(liveKitServerUtilService);
        });
    }

    [Fact]
    public async Task ShouldThrowExceptionWhenGuestNotAllow()
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            var currentUser = await repository.GetByIdAsync<UserAccount>(1);
            
            currentUser.Issuer = UserAccountIssuer.Guest;
            
            await repository.UpdateAsync(currentUser);
            
            await repository.InsertAsync(new Meeting
            {
                MeetingMasterUserId = 1,
                MeetingNumber = "123"

            });
        });
        
        await RunWithUnitOfWork<IMediator>(async mediator =>
        {
            await Assert.ThrowsAsync<GuestIsNotAllowException>(async () =>
            {
                await mediator.SendAsync(new EndMeetingCommand
                {
                    MeetingNumber = "123"
                });
            });
        }, builder =>
        {
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();
            
            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>()).Returns("");
            
            builder.RegisterInstance(liveKitServerUtilService);
        });
    }
}