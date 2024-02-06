using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.UnitTests;

public partial class BaseFixture
{
    protected Meeting CreateMeetingEvent(Guid meetingId, long? startDate = null, long? endDate = null,
        string meetingNumber = "5201314", string securityCode = "123456", string timeZone = "Asia/Shanghai",
        MeetingStatus status = MeetingStatus.Pending, bool isMuted = false, bool isRecorded = false)
    {
        return new Meeting
        {
            Id = meetingId,
            Title = "Greg Meeting",
            StartDate = startDate?? _clock.Now.ToUnixTimeSeconds(),
            EndDate = endDate ?? _clock.Now.AddDays(7).ToUnixTimeSeconds(),
            MeetingMasterUserId = 1,
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
    
    protected MeetingUserSession CreateUserSessionEvent(int id, int userId, Guid meetingId, DateTimeOffset? createdDate = null,
        MeetingAttendeeStatus status = MeetingAttendeeStatus.Absent, long firstJoinTime = 0, long lastQuitTime = 0, 
        int totalJoinCount = 0, long cumulativeTime = 0, bool isMuted = false, bool isSharingScreen = false)
    {
        return new MeetingUserSession
        {
            Id = id,
            UserId = userId,
            MeetingId = meetingId,
            Status = status,
            FirstJoinTime = firstJoinTime,
            LastQuitTime = lastQuitTime,
            TotalJoinCount = totalJoinCount,
            CumulativeTime = cumulativeTime,
            CreatedDate = createdDate ?? _clock.Now,
            IsMuted = isMuted,
            IsSharingScreen = isSharingScreen,
            IsDeleted = false
        };
    }
}