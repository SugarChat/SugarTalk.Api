using System;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Enums;
using Xunit;

namespace SugarTalk.Tests.IntegrationTests
{
    public class MeetingServiceFixture : TestBase
    {
        [Fact]
        public async Task ShouldScheduleMeeting()
        {
            var meetingId = Guid.NewGuid();
            
            var cmd = new ScheduleMeetingCommand
            {
                Id = meetingId,
                MeetingType = MeetingType.Adhoc
            };

            await Run<IMediator>(async mediator =>
            {
                var response = await mediator.SendAsync<ScheduleMeetingCommand, SugarTalkResponse<MeetingDto>>(cmd);

                response.Data.ShouldNotBeNull();
                response.Data.Id.ShouldBe(meetingId);
                response.Data.MeetingType.ShouldBe(MeetingType.Adhoc);
            });
        }
    }
}