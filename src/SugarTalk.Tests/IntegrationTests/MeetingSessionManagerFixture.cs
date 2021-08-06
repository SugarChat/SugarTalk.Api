using System;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using SugarTalk.Core.Entities;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Enums;
using SugarTalk.Messages.Requests.Meetings;
using Xunit;

namespace SugarTalk.Tests.IntegrationTests
{
    public class MeetingSessionManagerFixture : TestBase
    {
        [Fact]
        public async Task ShouldMeetingSessionSync()
        {
            var userSessionId = Guid.NewGuid().ToString();
            var userSessionIndex = Guid.NewGuid().ToString();

            var meeting = await CreateMeeting();
            
            await Run<IMeetingSessionService>(async meetingSessionService =>
            {
                var meetingSession = await meetingSessionService.GetMeetingSession(new GetMeetingSessionRequest
                {
                    MeetingNumber = meeting.MeetingNumber
                });
            });
        }

        private async Task<MeetingDto> CreateMeeting()
        {
            MeetingDto meeting = null;
            
            await Run<IMediator>(async mediator =>
            {
                var response = await mediator.SendAsync<ScheduleMeetingCommand, SugarTalkResponse<MeetingDto>>(new ScheduleMeetingCommand
                {
                    Id = Guid.NewGuid(),
                    MeetingType = MeetingType.Adhoc
                });

                meeting = response.Data;
            });

            return meeting;
        }
    }
}