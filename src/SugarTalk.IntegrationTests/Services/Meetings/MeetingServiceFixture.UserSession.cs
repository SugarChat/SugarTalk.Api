using Shouldly;
using StackExchange.Redis;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Meeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Livekit.IngressState.Types;

namespace SugarTalk.IntegrationTests.Services.Meetings
{
    public partial class MeetingServiceFixture
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task CanGetUserSessionPermissionsWhenReconnectMeeting(int masterUserId)
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

        [Theory]
        [InlineData("Test1", "123")]
        [InlineData("Test2", "123")]
        public async Task CanKickOutMeetingUserSession(string userName, string password)
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
            var testUser1 = await _accountUtil.AddUserAccount(userName, password);

            var joinMeetingDto1 = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
            joinMeetingDto1.UserSessionCount.ShouldBe(1);
            (joinMeetingDto1.MeetingMasterUserId == testUser1.Id).ShouldBeFalse();

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
            masterUser.UserSessionCount.ShouldBe(2);

            var kickOutMeetingResponse = await _meetingUtil.KickOutUserByUserIdAsync
                  (scheduleMeetingResponse.Data.Id, testUser1.Id, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber);
            kickOutMeetingResponse.Data.ShouldNotBeNull();
            kickOutMeetingResponse.Data.MeetingMasterUserId.ShouldBe(masterUser.MeetingMasterUserId);
            kickOutMeetingResponse.Data.UserSessionCount.ShouldBe(1);
            kickOutMeetingResponse.Data.UserSessions.FirstOrDefault(x => x.UserId == testUser1.Id).ShouldBeNull();
        }

        [Fact]
        public async Task KickOutUserStatusChange()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var testUser1 = await _accountUtil.AddUserAccount("Test1", "123");

            var joinMeetingDto1 = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);

            (joinMeetingDto1.MeetingMasterUserId == testUser1.Id).ShouldBeFalse();

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

            var kickOutMeetingResponse = await _meetingUtil.KickOutUserByUserIdAsync
                  (scheduleMeetingResponse.Data.Id, testUser1.Id, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber);

            var kickOutUserSession = await _meetingUtil.GetUserSessionAsync(testUser1.Id, scheduleMeetingResponse.Data.Id);
            kickOutUserSession.OnlineType.ShouldBe(MeetingUserSessionOnlineType.KickOutMeeting);
        }

        [Fact]
        public async Task CanKickOutUserRejoinsTheMeeting()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var testUser1 = await _accountUtil.AddUserAccount("Test1", "123");

            var joinMeetingDto1 = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);

            (joinMeetingDto1.MeetingMasterUserId == testUser1.Id).ShouldBeFalse();

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

            var kickOutMeetingResponse = await _meetingUtil.KickOutUserByUserIdAsync
                  (scheduleMeetingResponse.Data.Id, testUser1.Id, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber);

            var rejoinMeetingDto = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
            rejoinMeetingDto.UserSessionCount.ShouldBe(2);
            rejoinMeetingDto.UserSessions.FirstOrDefault(x => x.UserId == testUser1.Id).ShouldNotBeNull();
            (rejoinMeetingDto.UserSessions.FirstOrDefault(x => x.UserId == testUser1.Id)
                .OnlineType == MeetingUserSessionOnlineType.Online).ShouldBeTrue();
        }

        [Fact]
        public async Task WhenTheStatusChangeJoiningMeetingGeneratesNewUserSession()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var testUser1 = await _accountUtil.AddUserAccount("Test1", "123");

            var joinMeetingDto1 = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);

            (joinMeetingDto1.MeetingMasterUserId == testUser1.Id).ShouldBeFalse();

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

            var kickOutMeetingResponse = await _meetingUtil.KickOutUserByUserIdAsync
                  (scheduleMeetingResponse.Data.Id, testUser1.Id, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber);

            var rejoinMeetingDto = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
        }

        [Fact]
        public async Task WhenKickOutUserSessionTheGetUserSessionUpdate()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var testUser1 = await _accountUtil.AddUserAccount("Test1", "123");

            var joinMeetingDto1 = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);

            (joinMeetingDto1.MeetingMasterUserId == testUser1.Id).ShouldBeFalse();

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

            var kickOutMeetingResponse = await _meetingUtil.KickOutUserByUserIdAsync
                  (scheduleMeetingResponse.Data.Id, testUser1.Id, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber);
            var getMeetingRespone = await _meetingUtil.GetMeetingAsync(scheduleMeetingResponse.Data.MeetingNumber);

            getMeetingRespone.Data.UserSessionCount.ShouldBe(kickOutMeetingResponse.Data.UserSessionCount);
            getMeetingRespone.Data.UserSessions.FirstOrDefault(x => x.UserId == testUser1.Id).ShouldBeNull();
        }

        [Fact]
        public async Task WhenTheStatuChangeJoiningMeetingGeneratesNewUserSession()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var testUser1 = await _accountUtil.AddUserAccount("Test1", "123");

            var joinMeetingDto1 = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);

            (joinMeetingDto1.MeetingMasterUserId == testUser1.Id).ShouldBeFalse();

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

            var kickOutMeetingResponse = await _meetingUtil.KickOutUserByUserIdAsync
                  (scheduleMeetingResponse.Data.Id, testUser1.Id, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber);

            var newUserSession = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);

            var userSessions = await _meetingUtil.GetUserSessionByUserIdAsync(testUser1.Id, scheduleMeetingResponse.Data.Id);
            userSessions.Count.ShouldBe(2);
        }

        [Fact]
        public async Task WhenKickOutUserSessionRejoinMeetingTheUserSessionUpdate()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var testUser1 = await _accountUtil.AddUserAccount("Test1", "123");

            var joinMeetingDto = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);

            (joinMeetingDto.MeetingMasterUserId == testUser1.Id).ShouldBeFalse();

            var masterUser = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

            var kickOutMeetingResponse = await _meetingUtil.KickOutUserByUserIdAsync
                  (scheduleMeetingResponse.Data.Id, testUser1.Id, masterUser.MeetingMasterUserId, scheduleMeetingResponse.Data.MeetingNumber);

            var getMeetingRespone = await _meetingUtil.GetMeetingAsync(scheduleMeetingResponse.Data.MeetingNumber);
            getMeetingRespone.Data.UserSessionCount.ShouldBe(1);

            var rejoinMeetingDto = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
            var getRejoinMeetingRespone = await _meetingUtil.GetMeetingAsync(scheduleMeetingResponse.Data.MeetingNumber);
            getRejoinMeetingRespone.Data.UserSessionCount.ShouldBe(2);
            getRejoinMeetingRespone.Data.UserSessions.FirstOrDefault(x => x.UserId == testUser1.Id).ShouldNotBeNull();
        }

        [Fact]
        public async Task WhenRepeatJoiningMeetingTheUserSessionIsUnique()
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var testUser1 = await _accountUtil.AddUserAccount("Test1", "123");

            var joinMeetingDto = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
            var repeatJoinMeetingDto = await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber);
            var joinMeetingUserSession = joinMeetingDto.UserSessions.FirstOrDefault(x => x.UserId == testUser1.Id);
            var repeatJoinMeetingUserSession = repeatJoinMeetingDto.UserSessions.FirstOrDefault(x => x.UserId == testUser1.Id);

            joinMeetingUserSession.Id.ShouldBe(repeatJoinMeetingUserSession.Id);
        }
    }
}
