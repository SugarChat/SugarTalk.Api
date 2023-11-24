using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Mediator.Net;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.AntMediaServer;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.IntegrationTests.Utils.Meetings;

public class MeetingUtil : TestUtil
{
    public MeetingUtil(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<ScheduleMeetingResponse> ScheduleMeeting()
    {
        return await Run<IMediator, ScheduleMeetingResponse>(async (mediator) =>
        {
            var response = await mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(
                new ScheduleMeetingCommand
                {
                    StartDate = DateTimeOffset.Now,
                    EndDate = DateTimeOffset.Now.AddDays(1)
                });
            
            return response;
        }, builder =>
        {
            var meetingUtilService = Substitute.For<IAntMediaServerUtilService>();

            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            meetingUtilService.CreateMeetingAsync(Arg.Any<string>(), Arg.Any<CreateMeetingDto>(), Arg.Any<CancellationToken>())
                .Returns(new CreateMeetingResponseDto
                {
                    MeetingNumber = "123_ams"
                });

            liveKitServerUtilService.CreateMeetingAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(new CreateMeetingFromLiveKitResponseDto
                {
                    RoomInfo = new LiveKitRoom
                    {
                        MeetingNumber = "123_livekit"
                    }
                });
            
            builder.RegisterInstance(meetingUtilService);
            builder.RegisterInstance(liveKitServerUtilService);
        });
    }

    public async Task<MeetingDto> JoinMeeting(string meetingNumber, string streamId, bool isMuted = false)
    {
        return await Run<IMediator, MeetingDto>(async (mediator) =>
        {
            var response = await mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(new JoinMeetingCommand
            {
                MeetingNumber = meetingNumber,
                StreamId = streamId,
                IsMuted = isMuted
            });

            return response.Data.Meeting;
        }, builder =>
        {
            var antMediaServerUtilService = Substitute.For<IAntMediaServerUtilService>();

            antMediaServerUtilService.AddStreamToMeetingAsync(Arg.Any<string>(), Arg.Any<string>(),
                    Arg.Any<string>(), CancellationToken.None)
                .Returns(new ConferenceRoomResponseBaseDto { Success = true });

            builder.RegisterInstance(antMediaServerUtilService);
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
}