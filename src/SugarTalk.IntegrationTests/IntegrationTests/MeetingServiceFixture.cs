using System;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Enums;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Responses;
using SugarTalk.Tests.IntegrationTests.TestBaseClasses;
using Xunit;

namespace SugarTalk.Tests.IntegrationTests
{
    public class MeetingServiceFixture : FixtureBase
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

                var meetingSessionResponse =
                    await mediator.RequestAsync<GetMeetingSessionRequest, SugarTalkResponse<MeetingSession>>(
                        new GetMeetingSessionRequest
                        {
                            MeetingNumber = response.Data.MeetingNumber
                        });

                meetingSessionResponse.Data.ShouldNotBeNull();
            });
        }
    }
}