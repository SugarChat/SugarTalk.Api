using System;
using System.Threading.Tasks;
using Shouldly;
using SugarTalk.Messages.Enums;
using SugarTalk.Tests.TestBaseClasses;
using SugarTalk.Tests.Utils.Meeting;
using Xunit;

namespace SugarTalk.Tests.Services
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
    }
}