using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using Xunit;

namespace SugarTalk.UnitTests.Services.Meeting;

public class MeetingProcessJobServiceFixture : BaseFixture
{
    [Fact]
    public async Task ShouldCheckAppointmentMeetingDate()
    {
        var meeting1Id = Guid.NewGuid();
        var meeting2Id = Guid.NewGuid();
        var meeting3Id = Guid.NewGuid();

        _clock.Now.Returns(DateTimeOffset.Now);
        
        MockMeetingDb(_repository, new List<Core.Domain.Meeting.Meeting>
        {
            CreateMeetingEvent(meeting1Id, appointmentType: MeetingAppointmentType.Appointment,
                startDate: _clock.Now.AddMinutes(-1).ToUnixTimeSeconds(), endDate: _clock.Now.AddMinutes(30).ToUnixTimeSeconds(), status: MeetingStatus.Pending),
            CreateMeetingEvent(meeting2Id, appointmentType: MeetingAppointmentType.Appointment,
                startDate: _clock.Now.AddHours(-2).ToUnixTimeSeconds(), endDate: _clock.Now.AddHours(-1).ToUnixTimeSeconds(), status: MeetingStatus.InProgress),
            CreateMeetingEvent(meeting3Id, appointmentType: MeetingAppointmentType.Appointment,
                startDate: _clock.Now.AddHours(-2).ToUnixTimeSeconds(), endDate: _clock.Now.AddHours(-1).ToUnixTimeSeconds(), status: MeetingStatus.InProgress)
        });
        
        MockUserSessionDb(_repository, new List<MeetingUserSession>
        {
            CreateUserSessionEvent(id: 1, userId: 1, meeting3Id, status: MeetingAttendeeStatus.Present, onlineType: MeetingUserSessionOnlineType.Online)
        });

        await _meetingProcessJobService.CheckAppointmentMeetingDateAsync(new CheckAppointmentMeetingDateCommand(), CancellationToken.None);

        var meetings = await _repository.Query<Core.Domain.Meeting.Meeting>().ToListAsync();

        meetings.Count.ShouldBe(3);
        meetings.Count(x => x.Id == meeting1Id && x.Status == MeetingStatus.InProgress).ShouldBe(1);
        meetings.Count(x => x.Id == meeting2Id && x.Status == MeetingStatus.Completed).ShouldBe(1);
        meetings.Count(x => x.Id == meeting3Id && x.Status == MeetingStatus.InProgress).ShouldBe(1);
    }
}
