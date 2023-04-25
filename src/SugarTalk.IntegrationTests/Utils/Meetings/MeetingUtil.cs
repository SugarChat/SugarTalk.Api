using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Mediator.Net;
using NSubstitute;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.IntegrationTests.Utils.Meetings;

public class MeetingUtil : TestUtil
{
    public MeetingUtil(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<ScheduleMeetingResponse> ScheduleMeeting(Guid meetingId)
    {
        return await Run<IMediator, ScheduleMeetingResponse>(async (mediator) =>
        {
            var response = await mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(
                new ScheduleMeetingCommand
                {
                    MeetingType = MeetingType.Adhoc,
                    MeetingMode = StreamMode.MCU
                });
            
            return response;
        }, builder =>
        {
            var meetingUtilService = Substitute.For<IMeetingUtilService>();

            meetingUtilService.CreateMeetingAsync(Arg.Any<CreateMeetingDto>(), Arg.Any<CancellationToken>())
                .Returns(new CreateMeetingResponseDto()
                {
                    Id = meetingId,
                    MeetingNumber = "123",
                    Mode = "mcu",
                    MeetingType = MeetingType.Adhoc
                });
            
            builder.RegisterInstance(meetingUtilService);
        });
    }

    public async Task<GetMeetingSessionResponse> GetMeetingSession(string meetingNumber)
    {
        return await Run<IMediator, GetMeetingSessionResponse>(async (mediator) =>
        {
            var response = await mediator.RequestAsync<GetMeetingSessionRequest, GetMeetingSessionResponse>(
                new GetMeetingSessionRequest
                {
                    MeetingNumber = meetingNumber
                });

            return response;
        });
    }

    public async Task AddMeeting(Guid meetingId, string meetingNumber, MeetingType type)
    {
        await RunWithUnitOfWork<IRepository>(async (repository) =>
        {
            await repository.InsertAsync(new Meeting
            {
                Id = meetingId,
                MeetingNumber = meetingNumber,
                MeetingType = type
            }, CancellationToken.None).ConfigureAwait(false);
        });
    }

    public async Task AddMeetingSession(string meetingNumber, Guid? meetingId = default)
    {
        await RunWithUnitOfWork<IRepository>(async (repository) =>
        {
            await repository.InsertAsync(new MeetingSession
            {
                MeetingId = meetingId ?? Guid.NewGuid(),
                MeetingNumber = meetingNumber,
                MeetingType = MeetingType.Adhoc
            });
        }); 
    }
}