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

    protected UserAccount CreateUserAccountEvent(int userId)
    {
        return new UserAccount
        {
            Id = userId,
            Uuid = Guid.NewGuid(),
            UserName = "greg",
            Password = "123456",
            ThirdPartyUserId = Guid.NewGuid().ToString(),
            Issuer = UserAccountIssuer.Wiltechs,
            IsActive = true
        };
    }
}