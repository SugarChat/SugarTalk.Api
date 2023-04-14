using System;
using System.Threading.Tasks;
using Autofac;
using Mediator.Net;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Enums;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Tests.Utils.Meeting;

public class MeetingUtil : TestUtil
{
    public MeetingUtil(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<ScheduleMeetingResponse> ScheduleMeeting(Guid meetingId, MeetingType type)
    {
        return await Run<IMediator, ScheduleMeetingResponse>(async (mediator) =>
        {
            var response = await mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(
                new ScheduleMeetingCommand
                {
                    Id = meetingId,
                    MeetingType = type
                });

            return response;
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
}