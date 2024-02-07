using NSubstitute;
using SugarTalk.Core.Data;
using MockQueryable.NSubstitute;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;

namespace SugarTalk.UnitTests;

public partial class BaseFixture
{
    protected void MockMeetingDb(IRepository repository, List<Meeting> meetingList)
    {
        repository.Query<Meeting>().Returns(x => meetingList.AsQueryable().BuildMockDbSet());
    }
    
    protected void MockUserAccountDb(IRepository repository, List<UserAccount> userAccountList)
    {
        repository.Query<UserAccount>().Returns(x => userAccountList.AsQueryable().BuildMockDbSet());
    }

    protected void MockMeetingUserSessionDb(IRepository repository, List<MeetingUserSession> meetingUserSessions)
    {
        repository.Query<MeetingUserSession>().Returns(x => meetingUserSessions.AsQueryable().BuildMockDbSet());
    }

    protected void MockMeetingRecordDb(IRepository repository, List<MeetingRecord> meetingRecords)
    {
        repository.Query<MeetingRecord>().Returns(x => meetingRecords.AsQueryable().BuildMockDbSet());
    }
}