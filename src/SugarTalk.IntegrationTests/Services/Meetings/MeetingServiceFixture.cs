using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Autofac;
using Shouldly;
using NSubstitute;
using Mediator.Net;
using SugarTalk.Core.Data;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Extensions;
using SugarTalk.Core.Services.AntMediaServer;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.IntegrationTests.TestBaseClasses;
using SugarTalk.IntegrationTests.Utils.Account;
using SugarTalk.IntegrationTests.Utils.Meetings;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture : MeetingFixtureBase
{
    private readonly MeetingUtil _meetingUtil;
    private readonly AccountUtil _accountUtil;

    public MeetingServiceFixture()
    {
        _meetingUtil = new MeetingUtil(CurrentScope);
        _accountUtil = new AccountUtil(CurrentScope);
    }

    [Fact]
    public async Task ShouldScheduleMeeting()
    {
        var response = await _meetingUtil.ScheduleMeeting(
            title: "sugarTalk每周例会", timezone: "UTC", periodType: MeetingPeriodType.Weekly);

        response.Data.ShouldNotBeNull();
        response.Data.TimeZone.ShouldBe("UTC");
        response.Data.Title.ShouldBe("sugarTalk每周例会");
        response.Data.PeriodType.ShouldBe(MeetingPeriodType.Weekly);
        response.Data.MeetingStreamMode.ShouldBe(MeetingStreamMode.LEGACY);
    }

    [Fact]
    public async Task ShouldGetMeeting()
    {
        var meetingId = Guid.NewGuid();

        var meeting = new Core.Domain.Meeting.Meeting
        {
            Id = meetingId,
            MeetingNumber = "123"
        };
        
        await _meetingUtil.AddMeeting(meeting);

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
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            var response = await mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(new JoinMeetingCommand
            {
                MeetingNumber = scheduleMeetingResponse.Data.MeetingNumber,
                IsMuted = false
            });

            var meetingResult = await repository.Query<Core.Domain.Meeting.Meeting>()
                .Where(x => x.MeetingNumber == scheduleMeetingResponse.Data.MeetingNumber)
                .SingleAsync(CancellationToken.None);

            response.Data.Meeting.MeetingNumber.ShouldBe(meetingResult.MeetingNumber);
            response.Data.Meeting.MeetingStreamMode.ShouldBe(MeetingStreamMode.LEGACY);
            response.Data.Meeting.Id.ShouldBe(meetingResult.Id);
        }, builder =>
        {
            var antMediaServerUtilService = Substitute.For<IAntMediaServerUtilService>();

            antMediaServerUtilService.AddStreamToMeetingAsync(Arg.Any<string>(), Arg.Any<string>(),
                    Arg.Any<string>(), CancellationToken.None)
                .Returns(new ConferenceRoomResponseBaseDto { Success = true });

            builder.RegisterInstance(antMediaServerUtilService);
        });
    }

    [Fact]
    public async Task CanOutMeeting()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var meeting = await _meetingUtil.GetMeeting(scheduleMeetingResponse.Data.MeetingNumber);

        await _meetingUtil.JoinMeeting(meeting.MeetingNumber, "streamId");

        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            var beforeUserSession = await repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == meeting.Id).ToListAsync();

            beforeUserSession.Count.ShouldBe(1);

            await mediator.SendAsync<OutMeetingCommand, OutMeetingResponse>(new OutMeetingCommand
            {
                MeetingId = meeting.Id
            });

            var afterUserSession = await repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == meeting.Id).ToListAsync();

            afterUserSession.Count.ShouldBe(0);
        }, builder =>
        {
            var antMediaServerUtilService = Substitute.For<IAntMediaServerUtilService>();

            antMediaServerUtilService.RemoveStreamFromMeetingAsync(Arg.Any<string>(), Arg.Any<string>(),
                    Arg.Any<string>(), CancellationToken.None)
                .Returns(new ConferenceRoomResponseBaseDto { Success = true });

            builder.RegisterInstance(antMediaServerUtilService);
        });
    }

    [Fact]
    public async Task ShouldNotThrowWhenJoinMeetingDuplicated()
    {
        var isNotThrow = true;
        
        try
        {
            var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

            var meeting = await _meetingUtil.GetMeeting(scheduleMeetingResponse.Data.MeetingNumber);

            await _meetingUtil.JoinMeeting(meeting.MeetingNumber, "streamId1");
            await _meetingUtil.JoinMeeting(meeting.MeetingNumber, "streamId2");
        }
        catch (Exception ex)
        {
            isNotThrow = false;
        }
        
        isNotThrow.ShouldBeTrue();
    }

    [Fact]
    public async Task CanEndMeeting()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var meeting = await _meetingUtil.GetMeeting(scheduleMeetingResponse.Data.MeetingNumber);

        await _meetingUtil.JoinMeeting(meeting.MeetingNumber, "streamId");
        
        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            var beforeUserSession = await repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == meeting.Id).ToListAsync();

            beforeUserSession.Count.ShouldBe(1);

            await mediator.SendAsync<EndMeetingCommand, EndMeetingResponse>(new EndMeetingCommand
            {
                MeetingNumber = meeting.MeetingNumber
            });

            var afterUserSession = await repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == meeting.Id).ToListAsync();

            afterUserSession.Count.ShouldBe(0);
        }, builder =>
        {
            var antMediaServerUtilService = Substitute.For<IAntMediaServerUtilService>();

            antMediaServerUtilService
                .RemoveMeetingByMeetingNumberAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken.None)
                .Returns(new ConferenceRoomResponseBaseDto { Success = true });

            builder.RegisterInstance(antMediaServerUtilService);
        });
    }
    
    [Fact]
    public async Task CanGetMeetingByNumber()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var user1 = await _accountUtil.AddUserAccount("mars", "123");
        var user2 = await _accountUtil.AddUserAccount("greg", "123");

        await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber, "streamId");

        await Run<IMediator, IRepository, IUnitOfWork>(async (mediator, repository, unitOfWork) =>
        {
            await repository.InsertAllAsync(new List<MeetingUserSession>
            {
                new()
                {
                    UserId = user1.Id,
                    IsMuted = false,
                    MeetingId = scheduleMeetingResponse.Data.Id
                },
                new()
                {
                    UserId = user2.Id,
                    IsMuted = true,
                    MeetingId = scheduleMeetingResponse.Data.Id
                }
            });

            await unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            var response = await mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(
                new GetMeetingByNumberRequest
                {
                    MeetingNumber = scheduleMeetingResponse.Data.MeetingNumber
                });

            response.Data.ShouldNotBeNull();
            response.Data.UserSessions.Count.ShouldBe(3);
            response.Data.MeetingStreamMode.ShouldBe(MeetingStreamMode.LEGACY);
            response.Data.MeetingNumber.ShouldBe(scheduleMeetingResponse.Data.MeetingNumber);
            response.Data.UserSessions.Single(x => x.UserId == 1).UserName.ShouldBe("TEST_USER");
            response.Data.UserSessions.Single(x => x.UserId == user1.Id).UserName.ShouldBe("mars");
            response.Data.UserSessions.Single(x => x.UserId == user2.Id).UserName.ShouldBe("greg");
        });
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task CanShareScreen(bool isSharingScreen, bool expect)
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var meeting = await _meetingUtil.GetMeeting(scheduleMeetingResponse.Data.MeetingNumber);

        var user = await _accountUtil.AddUserAccount("test", "123");

        await _meetingUtil.AddMeetingUserSession(1, meeting.Id, 1);
        await _meetingUtil.AddMeetingUserSession(2, meeting.Id, user.Id, isSharingScreen: isSharingScreen);

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<ShareScreenCommand, ShareScreenResponse>(
                new ShareScreenCommand
                {
                    MeetingUserSessionId = 1,
                    StreamId = "123456",
                    IsShared = true
                });

            response.Data.MeetingUserSession.IsSharingScreen.ShouldBe(expect);

            if (!isSharingScreen)
            {
                response.Data.MeetingUserSession.UserSessionStreams.Count.ShouldBe(1);
                response.Data.MeetingUserSession.UserSessionStreams.Single().StreamId.ShouldBe("123456");
                response.Data.MeetingUserSession.UserSessionStreams.Single().MeetingUserSessionId.ShouldBe(1);
                response.Data.MeetingUserSession.UserSessionStreams.Single().StreamType.ShouldBe(MeetingStreamType.ScreenSharing);
            }
            else
                response.Data.MeetingUserSession.UserSessionStreams.ShouldNotBeNull();
        }, SetupMocking);
    }

    [Fact]
    public async Task ShouldNotChangeOtherUserAudio()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var meeting = await _meetingUtil.GetMeeting(scheduleMeetingResponse.Data.MeetingNumber);

        var user1 = await _accountUtil.AddUserAccount("test1", "123");
        var user2 = await _accountUtil.AddUserAccount("test2", "123");

        await Assert.ThrowsAsync<CannotChangeAudioWhenConfirmRequiredException>(async () =>
        {
            await Run<IMediator>(async (mediator) =>
            {
                await _meetingUtil.AddMeetingUserSession(1, meeting.Id, user1.Id);
                await _meetingUtil.AddMeetingUserSession(2, meeting.Id, user2.Id);
                
                await mediator.SendAsync<ChangeAudioCommand, ChangeAudioResponse>(
                    new ChangeAudioCommand
                    {
                        MeetingUserSessionId = 1,
                        StreamId = "123456",
                        IsMuted = true
                    });
            });
        });
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task CanChangeAudio(bool isMuted, bool expect)
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var joinMeeting =
            await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber, "streamId", !isMuted);

        var userSessionId = joinMeeting.UserSessions.Single().Id;

        await Run<IMediator>(async (mediator) =>
        {
            var response = await mediator.SendAsync<ChangeAudioCommand, ChangeAudioResponse>(
                new ChangeAudioCommand
                {
                    MeetingUserSessionId = userSessionId,
                    IsMuted = isMuted
                });

            response.Data.MeetingUserSession.IsMuted.ShouldBe(expect);

            response.Data.MeetingUserSession.UserSessionStreams.Count.ShouldBe(1);
            response.Data.MeetingUserSession.UserSessionStreams.Single().StreamId.ShouldBe("streamId");
            response.Data.MeetingUserSession.UserSessionStreams.Single().MeetingUserSessionId.ShouldBe(userSessionId);
            response.Data.MeetingUserSession.UserSessionStreams.Single().StreamType.ShouldBe(MeetingStreamType.Audio);
        }, SetupMocking);
    }

    [Fact]
    public async Task ShouldNotJoinMeetingWhenMeetingNotFound()
    {
        await Assert.ThrowsAsync<MeetingNotFoundException>(async () =>
        {
            await Run<IMediator>(async (mediator) =>
            {
                await mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(
                    new JoinMeetingCommand
                    {
                        MeetingNumber = "5201314",
                        StreamId = "123456",
                        IsMuted = true
                    });
            });
        });
    }

    [Fact]
    public async Task CanGetFullUserSessionWhenReconnectMeeting()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var beforeInfo = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber, "streamId");

        var afterInfo = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber, "streamId");

        afterInfo.UserSessions.Count.ShouldBe(beforeInfo.UserSessions.Count);
        afterInfo.UserSessions.Single().UserId.ShouldBe(beforeInfo.UserSessions.Single().UserId);
        afterInfo.UserSessions.Single().UserName.ShouldBe(beforeInfo.UserSessions.Single().UserName);
        afterInfo.UserSessions.Single().MeetingId.ShouldBe(beforeInfo.UserSessions.Single().MeetingId);
        afterInfo.UserSessions.Single().UserSessionStreams.Count.ShouldBe(beforeInfo.UserSessions.Single().UserSessionStreams.Count);
    }

    [Fact]
    public async Task CanGetMeetingWhenExcludeUserSession()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        
        await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber, "streamId");

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(
                new GetMeetingByNumberRequest
                {
                    IncludeUserSession = false,
                    MeetingNumber = scheduleMeetingResponse.Data.MeetingNumber
                });
    
            response.Data.ShouldNotBeNull();
    
            response.Data.AppName.ShouldBe("LiveApp");

            response.Data.UserSessions.ShouldBeNull();
            response.Data.StartDate.ShouldBe(scheduleMeetingResponse.Data.StartDate);
            response.Data.EndDate.ShouldBe(scheduleMeetingResponse.Data.EndDate);
            response.Data.MeetingNumber.ShouldBe(scheduleMeetingResponse.Data.MeetingNumber);
            response.Data.MeetingStreamMode.ShouldBe(scheduleMeetingResponse.Data.MeetingStreamMode);
        });
    }

    [Fact]
    public async Task CanUpdateMeeting()
    {
        var currentUser = new TestCurrentUser();

        var startDate = DateTimeOffset.Now;

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(), MeetingNumber = "123456",
            MeetingMasterUserId = currentUser.Id.Value, MeetingStreamMode = MeetingStreamMode.LEGACY,
            StartDate = startDate.ToUnixTimeSeconds(), EndDate = startDate.AddDays(5).ToUnixTimeSeconds()
        };
        
        await _meetingUtil.AddMeeting(meeting);

        meeting.Title = "greg meeting";
        meeting.SecurityCode = "123456";
        meeting.PeriodType = MeetingPeriodType.Weekly;
        meeting.TimeZone = "UTC";
        meeting.IsMuted = true;
        meeting.IsRecorded = true;

        await Run<IMediator>(async mediator =>
        {
            await mediator.SendAsync<UpdateMeetingCommand, UpdateMeetingResponse>(new UpdateMeetingCommand
            {
                Id = meeting.Id,
                Title = meeting.Title,
                SecurityCode = meeting.SecurityCode,
                PeriodType = meeting.PeriodType,
                StartDate = startDate.AddDays(7),
                EndDate = startDate.AddDays(8),
                TimeZone = meeting.TimeZone,
                IsMuted = meeting.IsMuted,
                IsRecorded = meeting.IsRecorded
            });
        });

        var response = await _meetingUtil.GetMeeting(meeting.MeetingNumber);
        
        response.Id.ShouldBe(meeting.Id);
        response.TimeZone.ShouldBe("UTC");
        response.Title.ShouldBe(meeting.Title);
        response.SecurityCode.ShouldBe("123456".ToSha256());
        response.PeriodType.ShouldBe(MeetingPeriodType.Weekly);
        response.MeetingNumber.ShouldBe(meeting.MeetingNumber);
        response.MeetingMasterUserId.ShouldBe(currentUser.Id.Value);
        response.StartDate.ShouldBe(startDate.AddDays(7).ToUnixTimeSeconds());
        response.EndDate.ShouldBe(startDate.AddDays(8).ToUnixTimeSeconds());
        response.IsMuted.ShouldBeTrue();
        response.IsRecorded.ShouldBeTrue();
    }

    private void SetupMocking(ContainerBuilder builder)
    {
        var antMediaServerUtilService = Substitute.For<IAntMediaServerUtilService>();

        antMediaServerUtilService.AddStreamToMeetingAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), CancellationToken.None)
            .Returns(new ConferenceRoomResponseBaseDto { Success = true });
            
        antMediaServerUtilService.RemoveStreamFromMeetingAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), CancellationToken.None)
            .Returns(new ConferenceRoomResponseBaseDto { Success = true });

        builder.RegisterInstance(antMediaServerUtilService); 
    }
}
