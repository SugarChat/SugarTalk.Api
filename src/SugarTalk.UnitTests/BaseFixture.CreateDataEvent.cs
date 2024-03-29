using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.UnitTests;

public partial class BaseFixture
{
    protected Meeting CreateMeetingEvent(Guid meetingId, long? startDate = null, long? endDate = null,
        string meetingNumber = "5201314", string title = "Greg Meeting", string securityCode = "123456", string timeZone = "Asia/Shanghai",
        MeetingAppointmentType appointmentType = MeetingAppointmentType.Quick, MeetingStatus status = MeetingStatus.Pending, 
        bool isMuted = false, bool isRecorded = false, int meetingMasterUserId = 1)

    {
        return new Meeting
        {
            Id = meetingId,
            Title = title,
            AppointmentType = appointmentType,
            StartDate = startDate?? _clock.Now.ToUnixTimeSeconds(),
            EndDate = endDate ?? _clock.Now.AddDays(7).ToUnixTimeSeconds(),
            MeetingMasterUserId = meetingMasterUserId,
            MeetingNumber = meetingNumber,
            OriginAddress = "https://localhost:6666",
            SecurityCode = securityCode,
            MeetingStreamMode = MeetingStreamMode.LEGACY,
            TimeZone = timeZone,
            Status = status,
            IsMuted = isMuted,
            IsRecorded = isRecorded
        };
    }

    protected UserAccount CreateUserAccountEvent(int userId, Guid uuid, string userName = "test_man")
    {
        return new UserAccount
        {
            Id = userId,
            Uuid = uuid,
            UserName = userName,
            Password = "123456",
            ThirdPartyUserId = Guid.NewGuid().ToString(),
            Issuer = UserAccountIssuer.Wiltechs,
            IsActive = true
        };
    }

    protected MeetingUserSession CreateUserSessionEvent(int id, int userId, Guid meetingId, Guid? meetingSubId = null, DateTimeOffset? createdDate = null,
        MeetingAttendeeStatus status = MeetingAttendeeStatus.Absent, MeetingUserSessionOnlineType onlineType = MeetingUserSessionOnlineType.Online, long firstJoinTime = 0, long lastQuitTime = 0,
        int totalJoinCount = 0, long cumulativeTime = 0, bool isMuted = false, bool isSharingScreen = false)
    {
        return new MeetingUserSession
        {
            Id = id,
            UserId = userId,
            MeetingId = meetingId,
            MeetingSubId = meetingSubId,
            Status = status,
            OnlineType = onlineType,
            LastJoinTime = firstJoinTime,
            LastQuitTime = lastQuitTime,
            TotalJoinCount = totalJoinCount,
            CumulativeTime = cumulativeTime,
            CreatedDate = createdDate ?? _clock.Now,
            IsMuted = isMuted,
            IsSharingScreen = isSharingScreen,
            IsDeleted = false
        };
    }

    protected MeetingRecord CreateMeetingRecordEvent(Guid id, Guid meetingId, string url, DateTimeOffset createDate)
    {
        return new MeetingRecord
        {
            Id = id,
            MeetingId = meetingId,
            Url = url,
            CreatedDate = createDate,
            RecordType = MeetingRecordType.OnRecord
        };
    }
    
    protected MeetingHistory CreateMeetingHistoryEvent(Guid id, Guid meetingId, int userId, Guid? meetingSubId, 
        long? joinTime = null, long duration = 1000)
    {
        return new MeetingHistory
        {
            Id = id,
            MeetingId = meetingId,
            MeetingSubId = meetingSubId,
            UserId = userId,
            CreatorJoinTime = joinTime ?? _clock.Now.ToUnixTimeSeconds(),
            Duration = duration
        };
    }
}