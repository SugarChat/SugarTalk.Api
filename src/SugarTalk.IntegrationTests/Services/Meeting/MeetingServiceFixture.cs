using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.IntegrationTests.TestBaseClasses;
using SugarTalk.IntegrationTests.Utils.Meetings;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Enums;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meeting;

public class MeetingServiceFixture : MeetingFixtureBase
{
    private readonly MeetingUtil _meetingUtil;

    public MeetingServiceFixture()
    {
        _meetingUtil = new MeetingUtil(CurrentScope);
    }

    [Fact]
    public async Task ShouldScheduleMeeting()
    {
        var meetingId = Guid.NewGuid();

        var response = await _meetingUtil.ScheduleMeeting(meetingId, MeetingType.Adhoc);

        response.Data.ShouldNotBeNull();
        response.Data.Id.ShouldBe(meetingId);
        response.Data.MeetingType.ShouldBe(MeetingType.Adhoc);

        var meetingSessionResponse = await _meetingUtil.GetMeetingSession(response.Data.MeetingNumber);

        meetingSessionResponse.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldGetMeeting()
    {
        var meetingId = Guid.NewGuid();

        await _meetingUtil.AddMeeting(meetingId, "123", MeetingType.Adhoc);

        await Run<IRepository>(async repository =>
        {
            var response = await repository
                .Query<Core.Domain.Meeting.Meeting>(x => x.Id == meetingId)
                .SingleAsync(CancellationToken.None);

            response.ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task CanJoinMeeting()
    {
        await _meetingUtil.AddMeetingSession("123");
        
        await _meetingUtil.AddMeeting(Guid.NewGuid(), "123", MeetingType.Adhoc);

        await RunWithUnitOfWork<IRepository>(async (repository) =>
        {
            var meetingSession = await repository.QueryNoTracking<MeetingSession>(x => x.MeetingNumber == "123").SingleAsync(CancellationToken.None);

            await repository.InsertAsync(new UserSession
            {
                UserName = "admin",
                UserId = new Guid("c2af213e-df6e-11ed-b5ea-0242ac120002"),
                MeetingSessionId = meetingSession.Id
            });
        }); 
        
        await Run<IMediator>(async (mediator) =>
        {
            var response = await mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(new JoinMeetingCommand
            {
                MeetingNumber = "123"
            });

            response.Data.ShouldNotBeNull();
            response.Data.UserSessions.Count.ShouldBe(1);
        });   
        
    }
}
