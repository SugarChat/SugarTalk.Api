using Shouldly;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Mediator.Net;
using SugarTalk.Core.Data;
using SugarTalk.Messages.Requests.Meetings;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meetings
{
    public partial class MeetingServiceFixture
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task CanGetMeetingUserSessionPermissionsWhenReconnectMeeting(int masterUserId)
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
            scheduleMeetingResponse.Data.MeetingMasterUserId = masterUserId;
            var beforeInfo = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
            beforeInfo.UserSessionCount.ShouldBe(1);
            beforeInfo.UserSessions.Single().UserId.ShouldBe(1);
            if (masterUserId == 1)
            {
                beforeInfo.UserSessions.FirstOrDefault(x => x.UserId == scheduleMeetingResponse.Data.MeetingMasterUserId).ShouldNotBeNull();

            }
            else
            {
                beforeInfo.UserSessions.FirstOrDefault(x => x.UserId == scheduleMeetingResponse.Data.MeetingMasterUserId).ShouldBeNull();
            }
        }

        [Fact]
        public async Task WhenVerifyMeetingUserPermissionShouldBeThrow()
        {

            var testUser1 = await _accountUtil.AddUserAccount("Test", "123456");
            var verifyMeetingUserPermissionCommand = new VerifyMeetingUserPermissionCommand
            {
                MeetingId = Guid.Parse("701A9BAB-B7CD-AD1B-CAC5-7A8963619B8D"),
                UserId = testUser1.Id
            };
            _meetingUtil.VerifyMeetingUserPermissionAsync(verifyMeetingUserPermissionCommand).ShouldThrow<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task WhenKickOutMeetingUserSessionShouldBeThrow()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
            var testUser1 = await _accountUtil.AddUserAccount("Test", "123456");

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
            masterUser.UserSessionCount.ShouldBe(1);
            
            var joinMeetingDto1 = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
            joinMeetingDto1.UserSessionCount.ShouldBe(2);
            (joinMeetingDto1.MeetingMasterUserId == testUser1.Id).ShouldBeFalse();

            _meetingUtil.KickOutUserByUserIdAsync
                     (scheduleMeetingResponse.Data.Id, 1, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber).ShouldThrow<CannotKickOutMeetingUserSessionException>();
        }

        [Theory]
        [InlineData("Test1", "123")]
        [InlineData("Test2", "123")]
        public async Task CanKickOutMeetingUserSession(string userName, string password)
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
            var testUser1 = await _accountUtil.AddUserAccount(userName, password);

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
            masterUser.UserSessionCount.ShouldBe(1);
            
            var joinMeetingDto1 = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
            joinMeetingDto1.UserSessionCount.ShouldBe(2);
            (joinMeetingDto1.MeetingMasterUserId == testUser1.Id).ShouldBeFalse();

            var kickOutMeetingResponse = await _meetingUtil.KickOutUserByUserIdAsync
                  (scheduleMeetingResponse.Data.Id, testUser1.Id, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber);
            kickOutMeetingResponse.Data.ShouldNotBeNull();
            kickOutMeetingResponse.Data.MeetingMasterUserId.ShouldBe(masterUser.MeetingMasterUserId);
            kickOutMeetingResponse.Data.UserSessionCount.ShouldBe(1);
            kickOutMeetingResponse.Data.UserSessions.FirstOrDefault(x => x.UserId == testUser1.Id).ShouldBeNull();
        }

        [Fact]
        public async Task CanKickOutMeetingUserStatusChange()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var testUser1 = await _accountUtil.AddUserAccount("Test1", "123");
            
            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

            var joinMeetingDto1 = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);

            (joinMeetingDto1.MeetingMasterUserId == testUser1.Id).ShouldBeFalse();

            var kickOutMeetingResponse = await _meetingUtil.KickOutUserByUserIdAsync
                  (scheduleMeetingResponse.Data.Id, testUser1.Id, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber);

            var kickOutUserSession = await _meetingUtil.GetUserSessionAsync(testUser1.Id, scheduleMeetingResponse.Data.Id);
            kickOutUserSession.OnlineType.ShouldBe(MeetingUserSessionOnlineType.KickOutMeeting);
        }

        [Fact]
        public async Task WhenKickOutUserSessionRejoinMeetingTheUserSessionUpdate()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var testUser1 = await _accountUtil.AddUserAccount("Test1", "123");

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
            
            var joinMeetingDto = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);

            (joinMeetingDto.MeetingMasterUserId == testUser1.Id).ShouldBeFalse();
            
            var kickOutMeetingResponse = await _meetingUtil.KickOutUserByUserIdAsync
                  (scheduleMeetingResponse.Data.Id, testUser1.Id, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber);

            var getMeetingRespone = await _meetingUtil.GetMeetingAsync(scheduleMeetingResponse.Data.MeetingNumber);
            getMeetingRespone.Data.UserSessionCount.ShouldBe(1);

            var getRejoinMeetingRespone = await _meetingUtil.GetMeetingAsync(scheduleMeetingResponse.Data.MeetingNumber);
            getRejoinMeetingRespone.Data.UserSessionCount.ShouldBe(1);
        }

        [Fact]
        public async Task WhenRepeatJoiningMeetingTheUserSessionIsUnique()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var testUser1 = await _accountUtil.AddUserAccount("Test1", "123");

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
            var joinMeetingDto = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
            var repeatJoinMeetingDto = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
            var joinMeetingUserSession = joinMeetingDto.UserSessions.FirstOrDefault(x => x.UserId == testUser1.Id);
            var repeatJoinMeetingUserSession = repeatJoinMeetingDto.UserSessions.FirstOrDefault(x => x.UserId == testUser1.Id);

            joinMeetingUserSession.Id.ShouldBe(repeatJoinMeetingUserSession.Id);
        }
        
        [Fact]
        public async Task ShouldThrowWhenJoinMeetingFromKickedOutUser()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var testUser1 = await _accountUtil.AddUserAccount("Test1", "123");
            var testUser2 = await _accountUtil.AddUserAccount("Test2", "123");

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
            await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
            await _meetingUtil.JoinMeetingByUserAsync(testUser2, scheduleMeetingResponse.Data.MeetingNumber);

            await _meetingUtil.KickOutUserByUserIdAsync
                (scheduleMeetingResponse.Data.Id, testUser1.Id, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber);
            
            await Run<IMediator, IRepository>(async (mediator, repository) =>
            {
                var response = await mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(new GetMeetingByNumberRequest
                {
                    MeetingNumber = scheduleMeetingResponse.Data.MeetingNumber
                });
                
                response.Data.UserSessions.Count.ShouldBe(2);

                await Assert.ThrowsAsync<CannotJoinMeetingWhenKickedOutMeetingException>(async () =>
                {
                    await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
                });
            });
        }
    }
}
