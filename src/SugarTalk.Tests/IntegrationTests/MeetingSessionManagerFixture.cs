using System;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using SugarTalk.Core.Services.Kurento;
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
            
            await Run<MeetingSessionManager>(async meetingSessionManager =>
            {
                var meetingSession = await meetingSessionManager.GetOrCreateMeetingSessionAsync(meeting);

                meetingSession.UserSessions.TryAdd(userSessionIndex, new UserSession
                {
                    Id = userSessionId,
                    UserId = DefaultUser.Id,
                    UserName = "test",
                    SendEndPoint = null,
                    ReceivedEndPoints = null
                });
                
                meetingSession.UserSessions.Count.ShouldBe(1);
            });
            
            await Run<MeetingSessionManager>(async meetingSessionManager =>
            {
                var meetingSession = await meetingSessionManager.GetOrCreateMeetingSessionAsync(meeting);

                meetingSession.UserSessions[userSessionIndex].IsSharingCamera = true;
            });

            await Run<IMediator>(async mediator =>
            {
                var response = await mediator.RequestAsync<GetMeetingSessionRequest, SugarTalkResponse<MeetingSession>>(
                    new GetMeetingSessionRequest
                    {
                        MeetingNumber = meeting.MeetingNumber
                    });

                response.Data.ShouldNotBeNull();
                response.Data.UserSessions[userSessionIndex].IsSharingCamera.ShouldBeTrue();
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