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
    
    protected void MockUserAccountsDb(IRepository repository, List<UserAccount> userAccountList)
    {
        repository.Query<UserAccount>().Returns(x => userAccountList.AsQueryable().BuildMockDbSet());
    }

    protected void MockMeetingRecordDb(IRepository repository, List<MeetingRecord> meetingRecords)
    {
        repository.Query<MeetingRecord>().Returns(x => meetingRecords.AsQueryable().BuildMockDbSet());
    }

    protected void MockUserSessionDb(IRepository repository, List<MeetingUserSession> userSessionList)
    {
        repository.Query<MeetingUserSession>().Returns(x => userSessionList.AsQueryable().BuildMockDbSet());
    }
    
    protected void MockMeetingHistoriesDb(IRepository repository, List<MeetingHistory> meetingHistories)
    {
        repository.Query<MeetingHistory>().Returns(x => meetingHistories.AsQueryable().BuildMockDbSet());
    }
}