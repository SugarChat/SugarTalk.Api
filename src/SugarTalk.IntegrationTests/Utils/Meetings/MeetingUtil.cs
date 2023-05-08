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
using SugarTalk.Messages.Dto.Meetings;
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
            var meetingUtilService = Substitute.For<IAntMediaUtilServer>();

            meetingUtilService.CreateMeetingAsync(Arg.Any<CreateMeetingDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new CreateMeetingResponseDto
                {
                    MeetingNumber = "123",
                    Mode = "mcu"
                });
            
            builder.RegisterInstance(meetingUtilService);
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
}