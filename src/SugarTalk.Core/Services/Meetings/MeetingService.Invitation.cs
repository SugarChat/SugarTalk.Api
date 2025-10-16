using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Dto.Smarties;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Enums.Smarties;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Services.Meetings;

public partial interface IMeetingService
{
    //添加邀请人
    Task<CreateMeetingInvitationRecordsResponse> CreateMeetingInvitationRecordsAsync(CreateMeetingInvitationRecordsCommand command, CancellationToken cancellationToken);
    
    //获取人员框架
    Task<GetMeetingInvitationUsersResponse> GetMeetingInvitationUsersAsync(GetMeetingInvitationUsersRequest request, CancellationToken cancellationToken);
    
    //修改状态
    Task UpdateMeetingInvitationRecordAsync();
}

public partial class MeetingService : IMeetingService
{
    public async Task<CreateMeetingInvitationRecordsResponse> CreateMeetingInvitationRecordsAsync(CreateMeetingInvitationRecordsCommand command, CancellationToken cancellationToken)
    {
        var accounts = await _accountDataProvider.GetUserAccountsAsync(userNames: command.Names, cancellationToken: cancellationToken).ConfigureAwait(false);

        var records = new List<MeetingInvitationRecord>();
        
        foreach (var account in accounts)
        {
            records.Add(new MeetingInvitationRecord
            {
                MeetingId = command.MeetingId,
                MeetingSubId = command.MeetingSubId,
                BeInviterUserId = account.Id,
                UserName = _currentUser.Name,
                UserId = _currentUser.Id ?? 0,
                InvitationStatus = MeetingInvitationStatus.InvitationPending
            });
        }
        
        Log.Information("Meeting invitation records: {@records}", records);

        await _meetingDataProvider.AddMeetingInvitationRecordsAsync(records, cancellationToken: cancellationToken).ConfigureAwait(false);

        return new CreateMeetingInvitationRecordsResponse
        {
            Data = _mapper.Map<MeetingInvitationRecordDto>(records)
        };
    }

    public async Task<GetMeetingInvitationUsersResponse> GetMeetingInvitationUsersAsync(GetMeetingInvitationUsersRequest request, CancellationToken cancellationToken)
    {
        var meetingUserSessions = await _meetingDataProvider.GetUserSessionsByMeetingIdAsync(meetingId: request.MeetingId, meetingSubId: request.MeetingSubId, includeUserName: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        var staffs = await _smartiesClient.GetStaffDepartmentHierarchyTreeAsync(new GetStaffDepartmentHierarchyTreeRequest
        {
            StaffIdSource = request.StaffIdSource,
            HierarchyDepth = request.HierarchyDepth,
            HierarchyStaffRange = request.HierarchyStaffRange
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("Invitation meeting user session: {@meetingUserSessions}", meetingUserSessions);
        
        UpdateStaffsAsync(staffs.Data.StaffDepartmentHierarchy, meetingUserSessions.Select(x => x.UserName).ToList());
        
        return new GetMeetingInvitationUsersResponse
        {
            Data = staffs.Data
        };
    }

    public static void UpdateStaffsAsync(List<GetStaffDepartmentHierarchyTreeRequestDto> staffDepartmentHierarchy, List<string> userNames)
    {
        foreach (var staff in staffDepartmentHierarchy)
        {
            if (staff.Staffs != null && staff.Staffs.Any())
            {
                staff.Staffs = staff.Staffs.Select(x =>
                {
                    if (userNames.Contains(x.UserName))
                    {
                        x.MeetingStaffStatus = MeetingStaffStatus.Online;
                    }

                    x.MeetingStaffStatus = MeetingStaffStatus.NoJoin;

                    return x;
                }).ToList();
            }else if (staff.Childrens != null && staff.Childrens.Any())
                UpdateStaffsAsync(staff.Childrens, userNames);
            
            return;
        }
    }

    public async Task UpdateMeetingInvitationRecordAsync()
    {
    }
}