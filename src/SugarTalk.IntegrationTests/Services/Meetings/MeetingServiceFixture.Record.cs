using System.Threading.Tasks;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Fact]
    public async Task CanGetMeetingRecord()
    {
        await _meetingUtil.ScheduleMeeting();
    }
}