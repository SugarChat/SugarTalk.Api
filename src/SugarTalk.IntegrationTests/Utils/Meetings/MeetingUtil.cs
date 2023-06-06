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
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;
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
                    MeetingStreamMode = MeetingStreamMode.MCU
                });
            
            return response;
        }, builder =>
        {
            var meetingUtilService = Substitute.For<IAntMediaServerUtilService>();

            meetingUtilService.CreateMeetingAsync(Arg.Any<string>(), Arg.Any<CreateMeetingDto>(), Arg.Any<CancellationToken>())
                .Returns(new CreateMeetingResponseDto
                {
                    MeetingNumber = "123",
                    Mode = "mcu"
                });
            
            builder.RegisterInstance(meetingUtilService);
        });
    }

    public async Task JoinMeeting(string meetingNumber, bool isMuted = false)
    {
        await Run<IMediator>(async (mediator) =>
        {
            await mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(new JoinMeetingCommand
            {
                MeetingNumber = meetingNumber,
                IsMuted = isMuted
            });
        }, builder =>
        {
            var antMediaServerUtilService = Substitute.For<IAntMediaServerUtilService>();

            antMediaServerUtilService.AddStreamToMeetingAsync(Arg.Any<string>(), Arg.Any<string>(),
                    Arg.Any<string>(), CancellationToken.None)
                .Returns(new ConferenceRoomResponseBaseDto { Success = true });

            builder.RegisterInstance(antMediaServerUtilService);
        });
    }

    public async Task AddMeeting(Guid meetingId, string meetingNumber)
    {
        await RunWithUnitOfWork<IRepository>(async (repository) =>
        {
            await repository.InsertAsync(new Meeting
            {
                Id = meetingId,
                MeetingNumber = meetingNumber,
            }, CancellationToken.None).ConfigureAwait(false);
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