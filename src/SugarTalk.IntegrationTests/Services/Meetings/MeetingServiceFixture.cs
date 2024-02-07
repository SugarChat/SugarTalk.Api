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
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Core.Services.Utils;
using SugarTalk.IntegrationTests.TestBaseClasses;
using SugarTalk.IntegrationTests.Utils.Account;
using SugarTalk.IntegrationTests.Utils.Meetings;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;
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
            title: "sugarTalk每周例会", timezone: "UTC", repeatType: MeetingRepeatType.Weekly);

        response.Data.ShouldNotBeNull();
        response.Data.TimeZone.ShouldBe("UTC");
        response.Data.Title.ShouldBe("sugarTalk每周例会");
        response.Data.RepeatType.ShouldBe(MeetingRepeatType.Weekly);
        response.Data.MeetingStreamMode.ShouldBe(MeetingStreamMode.LEGACY);
    }

    [Fact]
    public async Task ShouldGetMeeting()
    {
        var meetingId = Guid.NewGuid();

        var meeting = new Meeting
        {
            Id = meetingId,
            MeetingNumber = "123"
        };
        
        await _meetingUtil.AddMeeting(meeting);

        await Run<IRepository>(async repository =>
        {
            var response = await repository
                .Query<Meeting>(x => x.Id == meetingId)
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
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");

            builder.RegisterInstance(liveKitServerUtilService);
        });
    }

    [Fact]
    public async Task CanOutMeeting()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var meeting = await _meetingUtil.GetMeeting(scheduleMeetingResponse.Data.MeetingNumber);

        await _meetingUtil.JoinMeeting(meeting.MeetingNumber);

        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            var beforeUserSession = await repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == meeting.Id).ToListAsync();

            beforeUserSession.Count.ShouldBe(1);

            await mediator.SendAsync<OutMeetingCommand, OutMeetingResponse>(new OutMeetingCommand
            {
                MeetingId = meeting.Id
            });

            // todo：更新会议和会议中的用户状态
        }, builder =>
        {
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(liveKitServerUtilService);
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

            await _meetingUtil.JoinMeeting(meeting.MeetingNumber);
            await _meetingUtil.JoinMeeting(meeting.MeetingNumber);
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

        await _meetingUtil.JoinMeeting(meeting.MeetingNumber);
        
        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            var beforeUserSession = await repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == meeting.Id).ToListAsync();

            beforeUserSession.Count.ShouldBe(1);

            //Todo: 需要补充会议中的用户状态   
        });
    }
    
    [Fact]
    public async Task ShouldChangeStatusAfterEndMeeting()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting(securityCode: "123456");
        
        await Run<IMediator>(async (mediator) =>
        {
            var meetingInfo = await mediator.SendAsync<EndMeetingCommand, EndMeetingResponse>(
                new EndMeetingCommand
                {
                    MeetingNumber = scheduleMeetingResponse.Data.MeetingNumber
                });

            var meetingNumber = meetingInfo.Data.MeetingNumber;
            var response = await _meetingUtil.GetMeeting(meetingNumber);
            response.Status.ShouldBe(MeetingStatus.Completed);
            response.EndDate.ShouldNotBe(0);
        });
    }
    [Fact]
    public async Task CanGetMeetingByNumber()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var user1 = await _accountUtil.AddUserAccount("mars", "123");
        var user2 = await _accountUtil.AddUserAccount("greg", "123");

        await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

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

    [Theory(Skip = "已废弃")]
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
        }, SetupMocking);
    }

    [Fact(Skip = "已废弃")]
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

    [Theory(Skip = "已废弃")]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task CanChangeAudio(bool isMuted, bool expect)
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var joinMeeting =
            await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber, !isMuted);

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
                        IsMuted = true
                    });
            });
        });
    }

    [Fact]
    public async Task CanGetFullUserSessionWhenReconnectMeeting()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();

        var beforeInfo = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

        var afterInfo = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

        afterInfo.UserSessions.Count.ShouldBe(beforeInfo.UserSessions.Count);
        afterInfo.UserSessions.Single().UserId.ShouldBe(beforeInfo.UserSessions.Single().UserId);
        afterInfo.UserSessions.Single().UserName.ShouldBe(beforeInfo.UserSessions.Single().UserName);
        afterInfo.UserSessions.Single().MeetingId.ShouldBe(beforeInfo.UserSessions.Single().MeetingId);
    }

    [Fact]
    public async Task CanGetMeetingWhenExcludeUserSession()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        
        await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

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
        response.MeetingNumber.ShouldBe(meeting.MeetingNumber);
        response.MeetingMasterUserId.ShouldBe(currentUser.Id.Value);
        response.StartDate.ShouldBe(startDate.AddDays(7).ToUnixTimeSeconds());
        response.EndDate.ShouldBe(startDate.AddDays(8).ToUnixTimeSeconds());
        response.IsMuted.ShouldBeTrue();
        response.IsRecorded.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCannotJoinMeetingWhenInputIncorrectMeetingPassword()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting(securityCode: "123456");
        
        await Run<IMediator>(async (mediator) =>
        {
            var meetingInfo = await mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(
                new GetMeetingByNumberRequest
                {
                    MeetingNumber = scheduleMeetingResponse.Data.MeetingNumber,
                    IncludeUserSession = false
                });
            
            meetingInfo.Data.IsPasswordEnabled.ShouldBeTrue();

            await Assert.ThrowsAsync<MeetingSecurityCodeNotMatchException>(async () =>
            {
                await mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(new JoinMeetingCommand
                {
                    MeetingNumber = scheduleMeetingResponse.Data.MeetingNumber,
                    SecurityCode = "666"
                });
            });
        }, builder =>
        {
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(liveKitServerUtilService);
        });
    }

    [Theory]
    [InlineData(MeetingRepeatType.Daily, 29)]
    [InlineData(MeetingRepeatType.Weekly, 5)]
    [InlineData(MeetingRepeatType.BiWeekly, 3)]
    [InlineData(MeetingRepeatType.Monthly, 1)]
    [InlineData(MeetingRepeatType.EveryWeekday, 21)]
    public async Task CanCreateRepeatMeeting(MeetingRepeatType repeatType, int expectSubMeetingCount)
    {
        await Run<IMediator, IRepository ,IClock>(async (mediator, repository ,clock) =>
        {
            var command = new ScheduleMeetingCommand
            {
                Title = "Test Meeting",
                TimeZone = "Asia/Shanghai",
                SecurityCode = "123456",
                StartDate = clock.Now.AddHours(5),
                EndDate = clock.Now.AddHours(6),
                UtilDate = clock.Now.AddMonths(1),
                RepeatType = repeatType,
                AppointmentType = MeetingAppointmentType.Appointment
            };

            await mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(command);
            
            var meeting = await repository.Query<Meeting>().FirstOrDefaultAsync();
            meeting.ShouldNotBeNull();
        
            var meetingPeriodRule = await repository.Query<MeetingRepeatRule>().FirstOrDefaultAsync();
            meetingPeriodRule.ShouldNotBeNull();
            meetingPeriodRule.MeetingId.ShouldBe(meeting.Id);
            meetingPeriodRule.RepeatType.ShouldBe(command.RepeatType);
            meetingPeriodRule.RepeatUntilDate.ShouldBe(command.UtilDate.Value);

            var subMeetingList = await repository.Query<MeetingSubMeeting>().ToListAsync();
            subMeetingList.ShouldNotBeNull();
            subMeetingList.Count.ShouldBe(expectSubMeetingCount);
        }, builder =>
        {
            MockLiveKitService(builder);
            MockClock(builder, new DateTimeOffset(2024, 2, 1, 7, 0, 0, TimeSpan.Zero));
        });
    }
    
    [Fact]
    public async Task CanCreatePeriodMeetingWhenUtilDateIsNull()
    {
        await Run<IMediator, IRepository ,IClock>(async (mediator, repository ,clock) =>
        {
            var command = new ScheduleMeetingCommand
            {
                Title = "Test Meeting",
                TimeZone = "Asia/Shanghai",
                SecurityCode = "123456",
                StartDate = clock.Now.AddHours(5),
                EndDate = clock.Now.AddHours(6),
                UtilDate = null,
                RepeatType = MeetingRepeatType.Monthly,
                AppointmentType = MeetingAppointmentType.Appointment
            };

            await mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(command);
            
            var meeting = await repository.Query<Meeting>().FirstOrDefaultAsync();
            meeting.ShouldNotBeNull();
        
            var meetingPeriodRule = await repository.Query<MeetingRepeatRule>().FirstOrDefaultAsync();
            meetingPeriodRule.ShouldNotBeNull();
            meetingPeriodRule.MeetingId.ShouldBe(meeting.Id);
            meetingPeriodRule.RepeatType.ShouldBe(command.RepeatType);
            meetingPeriodRule.RepeatUntilDate.ShouldBeNull();

            var subMeetingList = await repository.Query<MeetingSubMeeting>().ToListAsync();
            subMeetingList.ShouldNotBeNull();
            subMeetingList.Count.ShouldBe(7);
        }, builder =>
        {
            MockLiveKitService(builder);
            MockClock(builder, new DateTimeOffset(2024, 2, 1, 7, 0, 0, TimeSpan.Zero));
        });
    }

    [Fact]
    public async Task CanUpdateMeetingWhenRepeatTypeChanged()
    {
        var meetingId = Guid.NewGuid();
        
        var now = new DateTimeOffset(2024, 2, 1, 7, 0, 0, TimeSpan.Zero);
        
        await Run<IMediator, IRepository, IClock>(async (mediator, repository, clock) =>
        {
            var command = new ScheduleMeetingCommand
            {
                Title = "Test Meeting",
                TimeZone = "UTC",
                SecurityCode = "123456",
                StartDate = clock.Now.AddHours(5),
                EndDate = clock.Now.AddHours(6),
                UtilDate = clock.Now.AddMonths(1),
                RepeatType = MeetingRepeatType.Daily,
                AppointmentType = MeetingAppointmentType.Appointment
            };

            await mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(command);

            var meeting = await repository.QueryNoTracking<Meeting>().FirstOrDefaultAsync();
            meeting.ShouldNotBeNull();
            meetingId = meeting.Id;

            var meetingPeriodRule = await repository.QueryNoTracking<MeetingRepeatRule>().FirstOrDefaultAsync();
            meetingPeriodRule.ShouldNotBeNull();
            meetingPeriodRule.MeetingId.ShouldBe(meeting.Id);
            meetingPeriodRule.RepeatType.ShouldBe(command.RepeatType);
            meetingPeriodRule.RepeatUntilDate.ShouldBe(command.UtilDate.Value);

            var subMeetingList = await repository.QueryNoTracking<MeetingSubMeeting>().ToListAsync();
            subMeetingList.ShouldNotBeNull();
            subMeetingList.Count.ShouldBe(29);
        }, builder =>
        {
            MockLiveKitService(builder);
            MockClock(builder, now);
        });
        
        await Run<IMediator, IRepository, IClock>(async (mediator, repository, clock) =>
        {
            var updateCommand = new UpdateMeetingCommand
            {
                Id = meetingId,
                Title = "Greg Meeting",
                TimeZone = "Asia/Shanghai",
                SecurityCode = "777888",
                StartDate = clock.Now.AddHours(5),
                EndDate = clock.Now.AddHours(6),
                UtilDate = clock.Now.AddMonths(1),
                RepeatType = MeetingRepeatType.Monthly,
                AppointmentType = MeetingAppointmentType.Appointment
            };

            await mediator.SendAsync<UpdateMeetingCommand, UpdateMeetingResponse>(updateCommand);

            var updatedMeeting = await repository.QueryNoTracking<Meeting>().FirstOrDefaultAsync();
            updatedMeeting.ShouldNotBeNull();
            updatedMeeting.Id.ShouldBe(meetingId);
            updatedMeeting.Title.ShouldBe(updateCommand.Title);
            updatedMeeting.AppointmentType.ShouldBe(updateCommand.AppointmentType);
            updatedMeeting.TimeZone.ShouldBe(updateCommand.TimeZone);
            updatedMeeting.SecurityCode.ShouldBe(updateCommand.SecurityCode.ToSha256());

            var updatedMeetingRepeatRule = await repository.QueryNoTracking<MeetingRepeatRule>().FirstOrDefaultAsync();
            updatedMeetingRepeatRule.ShouldNotBeNull();
            updatedMeetingRepeatRule.MeetingId.ShouldBe(meetingId);
            updatedMeetingRepeatRule.RepeatType.ShouldBe(updateCommand.RepeatType);
            updatedMeetingRepeatRule.RepeatUntilDate.ShouldBe(updateCommand.UtilDate.Value);

            var updatedSubMeetingList = await repository.QueryNoTracking<MeetingSubMeeting>()
                .Where(x => x.SubConferenceStatus == MeetingRecordSubConferenceStatus.Default).ToListAsync();
            updatedSubMeetingList.ShouldNotBeNull();
            updatedSubMeetingList.Count.ShouldBe(1);
        }, builder =>
        {
            MockLiveKitService(builder);
            MockClock(builder, now);
        });
    }

    private static void MockLiveKitService(ContainerBuilder builder)
    {
        var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

        liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
            .Returns("token123");

        liveKitServerUtilService.CreateMeetingAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new CreateMeetingFromLiveKitResponseDto
            {
                RoomInfo = new LiveKitRoom { MeetingNumber = "123_liveKit" }
            });

        builder.RegisterInstance(liveKitServerUtilService);
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
