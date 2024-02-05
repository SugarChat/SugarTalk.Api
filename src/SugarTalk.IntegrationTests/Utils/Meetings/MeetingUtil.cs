using System;
using Autofac;
using System.Linq;
using NSubstitute;
using Mediator.Net;
using System.Threading;
using SugarTalk.Core.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Core.Services.AntMediaServer;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.IntegrationTests.Utils.Meetings;

public class MeetingUtil : TestUtil
{
    public MeetingUtil(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<ScheduleMeetingResponse> ScheduleMeeting(
        string title = null, string timezone = null, string securityCode = null, 
        DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, 
        MeetingRepeatType repeatType = MeetingRepeatType.None, bool isMuted = false, bool isRecorded = false)
    {
        return await Run<IMediator, ScheduleMeetingResponse>(async (mediator) =>
        {
            var response = await mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(
                new ScheduleMeetingCommand
                {
                    Title = title,
                    TimeZone = timezone,
                    SecurityCode = securityCode,
                    StartDate = startDate ?? DateTimeOffset.Now,
                    EndDate = endDate ?? DateTimeOffset.Now.AddDays(2),
                    RepeatType = repeatType,
                    IsMuted = isMuted,
                    IsRecorded = isRecorded
                });
            
            return response;
        }, builder =>
        {
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            liveKitServerUtilService.CreateMeetingAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(new CreateMeetingFromLiveKitResponseDto
                {
                    RoomInfo = new LiveKitRoom
                    {
                        MeetingNumber = "123_livekit"
                    }
                });

            liveKitServerUtilService.GenerateTokenForCreateMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(liveKitServerUtilService);
        });
    }

    public async Task<MeetingDto> JoinMeeting(string meetingNumber, bool isMuted = false)
    {
        return await Run<IMediator, MeetingDto>(async (mediator) =>
        {
            var response = await mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(new JoinMeetingCommand
            {
                MeetingNumber = meetingNumber,
                IsMuted = isMuted
            });

            return response.Data.Meeting;
        }, builder =>
        {
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(liveKitServerUtilService);
        });
    }

    public async Task AddMeeting(Meeting meeting)
    {
        await RunWithUnitOfWork<IRepository>(async (repository) =>
        {
            await repository.InsertAsync(meeting, CancellationToken.None).ConfigureAwait(false);
        });
    }
    
    public async Task<Meeting> GetMeeting(string meetingNumber)
    {
        return await Run<IRepository, Meeting>(async (repository) =>
        {
            return await repository.QueryNoTracking<Meeting>()
                .Where(x => x.MeetingNumber == meetingNumber)
                .SingleAsync(CancellationToken.None);
        });
    }

    public async Task AddMeetingUserSession(int id, Guid meetingId, int userId, bool isMuted = false, bool isSharingScreen = false)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(new MeetingUserSession
            {
                Id = id,
                UserId = userId,
                IsMuted = isMuted,
                MeetingId = meetingId,
                IsSharingScreen = isSharingScreen
            }, CancellationToken.None);
        });
    }
    
    public async Task AddMeetingUserSetting(Guid id, int userId, Guid meetingId,
        SpeechTargetLanguageType targetLanguageType, CantoneseToneType? cantoneseToneType, DateTimeOffset? lastModifiedDate = null)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(new MeetingUserSetting
            {
                Id = id,
                UserId = userId,
                MeetingId = meetingId,
                TargetLanguageType = targetLanguageType,
                CantoneseToneType = cantoneseToneType ?? CantoneseToneType.HiuGaaiNeural,
                LastModifiedDate = lastModifiedDate ?? DateTimeOffset.Now
            }, CancellationToken.None);
        });
    }
}