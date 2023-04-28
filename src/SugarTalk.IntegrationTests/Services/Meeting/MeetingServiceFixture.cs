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
using SugarTalk.Messages.Enums.Meeting;
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

        var response = await _meetingUtil.ScheduleMeeting(meetingId);

        response.Data.ShouldNotBeNull();
        response.Data.Mode.ShouldBe("mcu");
    }

    [Fact]
    public async Task ShouldGetMeeting()
    {
        var meetingId = Guid.NewGuid();

        await _meetingUtil.AddMeeting(meetingId, "123");

        await Run<IRepository>(async repository =>
        {
            var response = await repository
                .Query<Core.Domain.Meeting.Meeting>(x => x.Id == meetingId)
                .SingleAsync(CancellationToken.None);

            response.ShouldNotBeNull();
        });
    }
}
