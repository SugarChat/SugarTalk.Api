using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.IntegrationTests.TestBaseClasses;
using SugarTalk.IntegrationTests.Utils.Meetings;
using SugarTalk.Messages.Enums;
using Xunit;

namespace SugarTalk.IntegrationTests.Services
{
    public class MeetingServiceFixture : FixtureBase
    {
        private readonly MeetingUtil _meetingUtil;

        public MeetingServiceFixture()
        {
            _meetingUtil = new MeetingUtil(CurrentScope);
        }

        [Fact]
        public async Task ShouldScheduleMeeting()
        {
            var meetingId = Guid.NewGuid();

            var response = await _meetingUtil.ScheduleMeeting(meetingId, MeetingType.Adhoc);

            response.Data.ShouldNotBeNull();
            response.Data.Id.ShouldBe(meetingId);
            response.Data.MeetingType.ShouldBe(MeetingType.Adhoc);

            var meetingSessionResponse = await _meetingUtil.GetMeetingSession(response.Data.MeetingNumber);

            meetingSessionResponse.Data.ShouldNotBeNull();
        }
        
        [Fact]
        public async Task ShouldGetMeeting()
        {
            var meetingId = Guid.NewGuid();

            await _meetingUtil.AddMeeting(meetingId, "123", MeetingType.Adhoc);
            
            await Run<IRepository>(async repository =>
            {
                var response = await repository
                    .Query<Meeting>(x => x.Id == meetingId)
                    .SingleAsync(CancellationToken.None);

                response.ShouldNotBeNull();
            });
        }
    }
}