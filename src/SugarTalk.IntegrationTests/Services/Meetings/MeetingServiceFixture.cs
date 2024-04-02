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
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.AntMediaServer;
using SugarTalk.Core.Services.Exceptions;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.Core.Services.Utils;
using SugarTalk.IntegrationTests.TestBaseClasses;
using SugarTalk.IntegrationTests.Utils.Account;
using SugarTalk.IntegrationTests.Utils.Meetings;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Commands.Meetings.Speak;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Dto;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Enums.Speech;
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
    public async Task CanChangeQuickMeetingTitle()
    {
        var response = await _meetingUtil.ScheduleMeeting(timezone: "UTC", appointmentType: MeetingAppointmentType.Quick);

        response.Data.ShouldNotBeNull();
        response.Data.TimeZone.ShouldBe("UTC");
        response.Data.Title.ShouldBe("TEST_USER的快速会议");
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
            var openAiService = Substitute.For<IOpenAiService>();
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");

            builder.RegisterInstance(liveKitServerUtilService);
            builder.RegisterInstance(openAiService);
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

            var afterUserSession = await repository.QueryNoTracking<MeetingUserSession>()
                .SingleAsync(x => x.MeetingId == meeting.Id);
            afterUserSession.OnlineType.ShouldBe(MeetingUserSessionOnlineType.OutMeeting);
            afterUserSession.LastQuitTime.ShouldNotBeNull();
            afterUserSession.LastQuitTime.Value.ShouldBeGreaterThanOrEqualTo(beforeUserSession.First().LastJoinTime.Value);
            afterUserSession.CumulativeTime.ShouldNotBeNull();
            afterUserSession.CumulativeTime.ShouldBe(afterUserSession.LastQuitTime.Value -
                                                     beforeUserSession.First().LastJoinTime.Value);
        }, builder =>
        {
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();
            var openAiService = Substitute.For<IOpenAiService>();
            
            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(openAiService);
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
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        
        var user1 = await _accountUtil.AddUserAccount("mars", "123");
        var user2 = await _accountUtil.AddUserAccount("greg", "123");
        
        await Run<IMediator, IRepository, IUnitOfWork>(async (mediator, repository, unitOfWork) =>
        {
            await repository.InsertAllAsync(new List<MeetingUserSession>
            {
                new()
                {
                    UserId = user1.Id,
                    IsMuted = false,
                    Status = MeetingAttendeeStatus.Present,
                    MeetingId = scheduleMeetingResponse.Data.Id
                },
                new()
                {
                    UserId = user2.Id,
                    IsMuted = true,
                    Status = MeetingAttendeeStatus.Present,
                    MeetingId = scheduleMeetingResponse.Data.Id
                }
            });
            
            await unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            
            var meetingInfo = await mediator.SendAsync<EndMeetingCommand, EndMeetingResponse>(
                new EndMeetingCommand
                {
                    MeetingNumber = scheduleMeetingResponse.Data.MeetingNumber
                });

            var meetingNumber = meetingInfo.Data.MeetingNumber;
            
            var response = await _meetingUtil.GetMeeting(meetingNumber);
            
            var beforeUserSession = await repository.QueryNoTracking<MeetingUserSession>()
                .Where(x => x.MeetingId == scheduleMeetingResponse.Data.Id).ToListAsync();
            
            beforeUserSession.Count.ShouldBe(2);
            beforeUserSession.ForEach(x =>
            {
                x.LastQuitTime.ShouldBe(response.EndDate);
                x.CumulativeTime.ShouldNotBeNull();
            });

            response.Status.ShouldBe(MeetingStatus.Completed);
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
    
            builder.RegisterInstance(openAiService);
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
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            
            builder.RegisterInstance(openAiService);
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
            }, builder =>
            {
                var openAiService = Substitute.For<IOpenAiService>();

                builder.RegisterInstance(openAiService);
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
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            
            builder.RegisterInstance(openAiService);
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
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
    
            builder.RegisterInstance(openAiService);
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

        var testUser1 = await _accountUtil.AddUserAccount("testMan", "123456");
        
        await Run<IMediator>(async (mediator) =>
        {
            var meetingInfo = await mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(
                new GetMeetingByNumberRequest
                {
                    MeetingNumber = scheduleMeetingResponse.Data.MeetingNumber,
                    IncludeUserSession = false
                });
            
            meetingInfo.Data.IsPasswordEnabled.ShouldBeTrue();

            await Assert.ThrowsAsync<MeetingSecurityCodeException>(async () =>
            {
                await _meetingUtil.JoinMeetingByUserAsync(testUser1, scheduleMeetingResponse.Data.MeetingNumber, securityCode: "666");
            });
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(openAiService);
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
            var openAiService = Substitute.For<IOpenAiService>();
            builder.RegisterInstance(openAiService);
            
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
            var openAiService = Substitute.For<IOpenAiService>();
            builder.RegisterInstance(openAiService);
            
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
            var openAiService = Substitute.For<IOpenAiService>();
    
            builder.RegisterInstance(openAiService);
            
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
            var openAiService = Substitute.For<IOpenAiService>();
    
            builder.RegisterInstance(openAiService);
            
            MockLiveKitService(builder);
            MockClock(builder, now);
        });
    }

    [Fact]
    public async Task CanGetMeetingHistories()
    {
        await Run<IMediator, IClock>(async (mediator, clock) =>
        {
            var meeting1Response = await _meetingUtil.ScheduleMeeting(appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.Daily, startDate: clock.Now, endDate: clock.Now.AddHours(1));
            var meeting2Response = await _meetingUtil.ScheduleMeeting(startDate: clock.Now, endDate: clock.Now.AddMinutes(30));
            var meeting3Response = await _meetingUtil.ScheduleMeeting(startDate: clock.Now, endDate: clock.Now.AddMinutes(15));

            await _meetingUtil.JoinMeeting(meeting1Response?.Data.MeetingNumber);
            await _meetingUtil.JoinMeeting(meeting2Response?.Data.MeetingNumber);
            await _meetingUtil.JoinMeeting(meeting3Response?.Data.MeetingNumber);

            await _meetingUtil.EndMeeting(meeting1Response?.Data.MeetingNumber);
            await _meetingUtil.EndMeeting(meeting2Response?.Data.MeetingNumber);
            await _meetingUtil.EndMeeting(meeting3Response?.Data.MeetingNumber);

            var response1 = await mediator.RequestAsync<GetMeetingHistoriesByUserRequest, GetMeetingHistoriesByUserResponse>(new GetMeetingHistoriesByUserRequest());
            response1.MeetingHistoryList.ShouldNotBeNull();
            response1.MeetingHistoryList.Count.ShouldBe(3);
            response1.TotalCount.ShouldBe(3);
            response1.MeetingHistoryList.Single(x => x.MeetingId == meeting1Response?.Data.Id).attendees.Count.ShouldBe(1);
            response1.MeetingHistoryList.Single(x => x.MeetingId == meeting1Response?.Data.Id).MeetingSubId.ShouldNotBeNull();

            var response2 =
                await mediator.RequestAsync<GetMeetingHistoriesByUserRequest, GetMeetingHistoriesByUserResponse>(
                    new GetMeetingHistoriesByUserRequest { PageSetting = new PageSetting { Page = 1, PageSize = 2 } });
            response2.MeetingHistoryList.ShouldNotBeNull();
            response2.MeetingHistoryList.Count.ShouldBe(2);
            response2.TotalCount.ShouldBe(3);
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            
            builder.RegisterInstance(openAiService);
            
            MockLiveKitService(builder);
            MockClock(builder, DateTimeOffset.Now);
        });
    }

    [Fact]
    public async Task ShouldThrowWhenAppointmentMeetingUtilDateIncorrect()
    {
        await Assert.ThrowsAsync<CannotCreateRepeatMeetingWhenUtilDateIsBeforeNowException>(async () =>
        {
            await Run<IClock>(async (clock) =>
            {
                await _meetingUtil.ScheduleMeeting(
                    appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.Daily,
                    startDate: clock.Now, endDate: clock.Now.AddHours(1), utilDate: clock.Now.AddDays(-1));
            }, builder =>
            {
                MockLiveKitService(builder);
                MockClock(builder, DateTimeOffset.Now);
            });
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CanCancelAppointmentMeeting(bool isJoinMeeting)
    {
        var meeting = await _meetingUtil.ScheduleMeeting(appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.Daily);
        meeting.Data.Status.ShouldBe(MeetingStatus.Pending);

        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            if (isJoinMeeting)
            {
                await _meetingUtil.JoinMeeting(meeting.Data.MeetingNumber);

                await Assert.ThrowsAsync<CannotCancelAppointmentMeetingStatusException>(async () =>
                {
                    await mediator.SendAsync(new CancelAppointmentMeetingCommand { MeetingId = meeting.Data.Id });
                });
            }
            else
            {
                await mediator.SendAsync(new CancelAppointmentMeetingCommand { MeetingId = meeting.Data.Id });

                (await repository.Query<Meeting>().FirstOrDefaultAsync())?.Status.ShouldBe(MeetingStatus.Cancelled);
            }
        });
    }
    
    [Fact]
    public async Task CanCheckAttendeeWhenJoinMeeting()
    {
        await Run<IMediator, IClock>(async (mediator, clock) =>
        {
            var meeting = await _meetingUtil.ScheduleMeeting(startDate: clock.Now.AddHours(1), endDate: clock.Now.AddHours(2));

            await Assert.ThrowsAsync<CannotJoinMeetingWhenMeetingClosedException>(async () =>
            {
                await mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(new JoinMeetingCommand
                {
                    MeetingNumber = meeting.Data.MeetingNumber
                });
            });
        }, builder =>
        {
            MockLiveKitService(builder);
            MockClock(builder, DateTimeOffset.Now);
            
            var accountDataProvider = Substitute.For<IAccountDataProvider>();
            accountDataProvider.GetUserAccountAsync(Arg.Any<int>()).Returns(new UserAccountDto
            {
                Id = 2
            });
            builder.RegisterInstance(accountDataProvider);
        });
    }
    
    [Fact]
    public async Task CanGetAppointmentMeetings()
    {
        await RunWithUnitOfWork<IMediator, IClock>(async (mediator, clock) =>
        {
            var meeting1Response = await _meetingUtil.ScheduleMeeting("预定会议有重复子会议", appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.Daily, startDate: clock.Now.AddDays(-1), endDate: clock.Now.AddDays(-1).AddHours(1));
            var meeting2Response = await _meetingUtil.ScheduleMeeting("预定会议有重复子会议", appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.Weekly, startDate: clock.Now, endDate: clock.Now.AddHours(1));
            var meeting3Response = await _meetingUtil.ScheduleMeeting("预定会议有重复子会议",appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.BiWeekly, startDate: clock.Now.AddDays(2), endDate: clock.Now.AddDays(2).AddHours(1));
            var meeting4Response = await _meetingUtil.ScheduleMeeting("预定会议没有重复子会议1",appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.None, startDate: clock.Now.AddDays(-1).AddHours(-3), endDate: clock.Now.AddDays(-1).AddHours(-2));
            var meeting5Response = await _meetingUtil.ScheduleMeeting("预定会议没有重复子会议2",appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.None, startDate: clock.Now.AddHours(1), endDate: clock.Now.AddHours(2));
            
            var response = await mediator.RequestAsync<GetAppointmentMeetingsRequest, GetAppointmentMeetingsResponse>(
                new GetAppointmentMeetingsRequest
                {
                    Page = 1, PageSize = 10
                });

            response.Data.Count.ShouldBe(4);
            response.Data.Records.Count(x => x.MeetingId == meeting1Response.Data.Id).ShouldBe(1);
            response.Data.Records.Count(x => x.MeetingId == meeting2Response.Data.Id).ShouldBe(1);
            response.Data.Records.Count(x => x.MeetingId == meeting3Response.Data.Id).ShouldBe(1);
            response.Data.Records.Count(x => x.MeetingId == meeting5Response.Data.Id && x.Title == "预定会议没有重复子会议2").ShouldBe(1);
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();          
            
            builder.RegisterInstance(openAiService);
            MockClock(builder, DateTimeOffset.Now);
        });
    }
    
    [Fact]
    public async Task CanUpdateRepeatMeeting()
    {
        await RunWithUnitOfWork<IRepository, IMeetingProcessJobService>(async (repository, meetingProcessJobService) =>
        {
            var meeting1Response = await _meetingUtil.ScheduleMeeting(appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.Daily, startDate: DateTimeOffset.Parse("2024-02-23T10:00:00"), endDate: DateTimeOffset.Parse("2024-02-23T11:00:00"));
            var meeting2Response = await _meetingUtil.ScheduleMeeting(appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.Weekly, startDate: DateTimeOffset.Parse("2024-02-23T10:00:00"), endDate: DateTimeOffset.Parse("2024-02-23T11:00:00"));
            var meeting3Response = await _meetingUtil.ScheduleMeeting(appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.BiWeekly, startDate: DateTimeOffset.Parse("2024-02-23T10:00:00"), endDate: DateTimeOffset.Parse("2024-02-23T11:00:00"));

            meeting1Response.Data.Status.ShouldBe(MeetingStatus.Pending);
            
            var joinMeeting = await _meetingUtil.JoinMeeting(meeting1Response.Data.MeetingNumber);
            joinMeeting.Status.ShouldBe(MeetingStatus.InProgress);

            await meetingProcessJobService.UpdateRepeatMeetingAsync(new UpdateRepeatMeetingCommand(), CancellationToken.None);

            var meetings = await repository.Query<Meeting>().ToListAsync(CancellationToken.None).ConfigureAwait(false);
            meetings.Count.ShouldBe(3);

            meetings.Count(x =>
                x.MeetingNumber == meeting1Response.Data.MeetingNumber &&
                x.Status == MeetingStatus.Pending &&
                x.StartDate == DateTimeOffset.Parse("2024-02-24T10:00:00").ToUnixTimeSeconds()).ShouldBe(1);
            
            meetings.Count(x =>
                x.MeetingNumber == meeting2Response.Data.MeetingNumber &&
                x.Status == MeetingStatus.Pending &&
                x.StartDate == DateTimeOffset.Parse("2024-03-01T10:00:00").ToUnixTimeSeconds()).ShouldBe(1);
            
            meetings.Count(x =>
                x.MeetingNumber == meeting3Response.Data.MeetingNumber &&
                x.Status == MeetingStatus.Pending &&
                x.StartDate == DateTimeOffset.Parse("2024-03-08T10:00:00").ToUnixTimeSeconds()).ShouldBe(1);
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();          
            
            builder.RegisterInstance(openAiService);
            MockClock(builder, new DateTimeOffset(2024, 2, 24, 0, 1, 0, TimeSpan.Zero));
        });
    }
    
    [Fact]
    public async Task CanGetMeetingInviteInfo()
    {
        await _meetingUtil.ScheduleMeeting("sugarTalk每周例会", securityCode: "123456");
        
        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            var meeting = await repository.QueryNoTracking<Meeting>().FirstOrDefaultAsync();
            meeting.ShouldNotBeNull();
            
            var meetingInviteInfo = await mediator.RequestAsync<GetMeetingInviteInfoRequest, GetMeetingInviteInfoResponse>(
                new GetMeetingInviteInfoRequest
                {
                    MeetingId = meeting.Id
                });

            meetingInviteInfo.ShouldNotBeNull();
            meetingInviteInfo.Data.Sender.ShouldBe("TEST_USER");
            meetingInviteInfo.Data.Title.ShouldBe(meeting.Title);
            meetingInviteInfo.Data.MeetingNumber.ShouldBe(meeting.MeetingNumber);
            meetingInviteInfo.Data.Password.ShouldBe(meeting.Password);
        });
    }
    
    [Fact]
    public async Task ShouldThrowMeetingIsNoPendingWhenUpdateMeeting()
    {
        var meeting1Response = await _meetingUtil.ScheduleMeeting(appointmentType: MeetingAppointmentType.Appointment, repeatType: MeetingRepeatType.Daily, startDate: DateTimeOffset.Parse("2024-02-23T10:00:00"), endDate: DateTimeOffset.Parse("2024-02-23T11:00:00"));
        
        await _meetingUtil.JoinMeeting(meeting1Response.Data.MeetingNumber);
        
        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            var meeting = await repository.QueryNoTracking<Meeting>().FirstOrDefaultAsync();
            meeting.ShouldNotBeNull();
            meeting.Status.ShouldBe(MeetingStatus.InProgress);

            await Assert.ThrowsAsync<CannotUpdateMeetingWhenStatusNotPendingException>(async () =>
            {
                await mediator.SendAsync<UpdateMeetingCommand, UpdateMeetingResponse>(new UpdateMeetingCommand
                {
                    Id = meeting.Id,
                    Title = "Greg Meeting",
                    TimeZone = "Asia/Shanghai",
                    SecurityCode = "777888",
                    StartDate = DateTimeOffset.Parse("2024-02-23T10:00:00"),
                    EndDate = DateTimeOffset.Parse("2024-02-23T11:00:00"),
                    UtilDate = DateTimeOffset.Parse("2024-02-23T10:00:00"),
                    RepeatType = MeetingRepeatType.Daily,
                    AppointmentType = MeetingAppointmentType.Appointment
                });
            });
        });
    }
    
    [Fact]
    public async Task CanGetMeetingGuestWhenJoinMeeting()
    {
        var meeting = await _meetingUtil.ScheduleMeeting();
            
        var testUser1 = await _accountUtil.AddUserAccount("guest1", "123456", issuer: UserAccountIssuer.Guest);
        var testUser2 = await _accountUtil.AddUserAccount("guest2", "111222", issuer: UserAccountIssuer.Guest);
        var testUser3 = await _accountUtil.AddUserAccount("guest3", "222244", issuer: UserAccountIssuer.Guest);
        var testUser4 = await _accountUtil.AddUserAccount("guest4", "222666", issuer: UserAccountIssuer.Guest);

        await _meetingUtil.JoinMeeting(meeting.Data.MeetingNumber);

        await _meetingUtil.JoinMeetingByUserAsync(testUser1, meeting.Data.MeetingNumber);

        await Task.Delay(50);
            
        await _meetingUtil.JoinMeetingByUserAsync(testUser2, meeting.Data.MeetingNumber);
        
        await Task.Delay(50);
        
        await _meetingUtil.JoinMeetingByUserAsync(testUser3, meeting.Data.MeetingNumber);
        
        await Run<IMediator>(async (mediator) =>
        {
            var response1 = await mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(
                new GetMeetingByNumberRequest
                {
                    MeetingNumber = meeting.Data.MeetingNumber
                });
            
            response1.Data.UserSessions.Count.ShouldBe(4);
            response1.Data.UserSessions.Single(x=>x.UserName == testUser1.UserName).GuestName.ShouldBe("Anonymity1");
            response1.Data.UserSessions.Single(x=>x.UserName == testUser2.UserName).GuestName.ShouldBe("Anonymity2");
            response1.Data.UserSessions.Single(x=>x.UserName == testUser3.UserName).GuestName.ShouldBe("Anonymity3");

            await _meetingUtil.OutMeetingByUser(testUser2, meeting.Data.Id);
            
            var response2 = await mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(
                new GetMeetingByNumberRequest
                {
                    MeetingNumber = meeting.Data.MeetingNumber
                });
            
            response2.Data.UserSessions.Count.ShouldBe(3);
            response2.Data.UserSessions.Single(x => x.UserName == testUser3.UserName).GuestName.ShouldBe("Anonymity3");

            await _meetingUtil.JoinMeetingByUserAsync(testUser4, meeting.Data.MeetingNumber);

            var response3 = await mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(
                new GetMeetingByNumberRequest
                {
                    MeetingNumber = meeting.Data.MeetingNumber
                });
            
            response3.Data.UserSessions.Count.ShouldBe(4);
            response3.Data.UserSessions.Single(x => x.UserName == testUser4.UserName).GuestName.ShouldBe("Anonymity4");

            await _meetingUtil.JoinMeetingByUserAsync(testUser2, meeting.Data.MeetingNumber);

            var response4 = await mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(
                new GetMeetingByNumberRequest
                {
                    MeetingNumber = meeting.Data.MeetingNumber
                });
            
            response4.Data.UserSessions.Count.ShouldBe(5);
            response4.Data.UserSessions.Single(x => x.UserName == testUser2.UserName).GuestName.ShouldBe("Anonymity5");
        }, MockLiveKitService);
    }

    private static void MockLiveKitService(ContainerBuilder builder)
    {
        var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

        liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
            .Returns("token123");

        liveKitServerUtilService.CreateMeetingAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new CreateMeetingFromLiveKitResponseDto());

        builder.RegisterInstance(liveKitServerUtilService);
    }
    
    private void SetupMocking(ContainerBuilder builder)
    {
        var openAiService = Substitute.For<IOpenAiService>();
        var antMediaServerUtilService = Substitute.For<IAntMediaServerUtilService>();

        antMediaServerUtilService.AddStreamToMeetingAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), CancellationToken.None)
            .Returns(new ConferenceRoomResponseBaseDto { Success = true });
            
        antMediaServerUtilService.RemoveStreamFromMeetingAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), CancellationToken.None)
            .Returns(new ConferenceRoomResponseBaseDto { Success = true });

        builder.RegisterInstance(antMediaServerUtilService);
        builder.RegisterInstance(openAiService);
    }

    [Theory]
    [InlineData("8b9c631a-3c76-4b24-b90d-5a25d6b2f4f9", true, false, "", SpeechTargetLanguageType.Cantonese, SpeechTargetLanguageType.Cantonese)]
    [InlineData("8b9c631a-3c76-4b24-b90d-5a25d6b2f4f9", false, false, "123456", SpeechTargetLanguageType.English, SpeechTargetLanguageType.English)]
    [InlineData("af0e2598-7d9d-493e-bf77-6f33db8f5c3c", false, false, "123456", SpeechTargetLanguageType.English, SpeechTargetLanguageType.English)]
    public async Task CanAddOrUpdateMeetingChatRoom(
        Guid meetingId, bool roomSettingIsSystem, bool commandIsSystem, string voiceId, SpeechTargetLanguageType listeningLanguage, SpeechTargetLanguageType selfLanguage)
    {
        var meetingChatRoomSetting = new MeetingChatRoomSetting()
        {
            UserId = 1,
            MeetingId = Guid.Parse("8b9c631a-3c76-4b24-b90d-5a25d6b2f4f9"),
            VoiceId = "123",
            IsSystem = roomSettingIsSystem,
            LastModifiedDate = DateTimeOffset.Now
        };
        
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(meetingChatRoomSetting).ConfigureAwait(false);
        }).ConfigureAwait(false);

        await Run<IMediator, IRepository>(async (mediator, repository) =>
        {
            await mediator.SendAsync(new AddOrUpdateChatRoomSettingCommand
            {
                MeetingId = meetingId,
                IsSystem = commandIsSystem,
                VoiceId = voiceId,
                Gender = EchoAvatarGenderVoiceType.Female,
                VoiceName = "yayaya",
            }).ConfigureAwait(false);

            var meetingChatRoom = await repository.QueryNoTracking<MeetingChatRoomSetting>(
                x => x.UserId == 1 && x.MeetingId == meetingId).FirstOrDefaultAsync().ConfigureAwait(false);
            
            meetingChatRoom.ShouldNotBeNull();

            if (commandIsSystem)
            {
                if (roomSettingIsSystem)
                {
                    meetingChatRoom.SelfLanguage.ShouldBe(meetingChatRoomSetting.SelfLanguage);
                    meetingChatRoom.ListeningLanguage.ShouldBe(meetingChatRoomSetting.ListeningLanguage);
                    meetingChatRoom.VoiceId.ShouldNotBeEmpty();
                }
                else
                {
                    meetingChatRoom.SelfLanguage.ShouldBe(selfLanguage);
                    meetingChatRoom.ListeningLanguage.ShouldBe(listeningLanguage);
                    meetingChatRoom.VoiceId.ShouldBe(voiceId);
                }
            }
        });
    }
}
