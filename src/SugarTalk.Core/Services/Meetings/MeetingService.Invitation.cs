using System;
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
    Task<UpdateMeetingInvitationRecordsResponse> UpdateMeetingInvitationRecordAsync(UpdateMeetingInvitationRecordsCommand command, CancellationToken cancellationToken);
    
    //获取邀请信息
    Task<GetMeetingInvitationRecordsResponse> GetMeetingInvitationRecordsAsync(GetMeetingInvitationRecordsRequest request, CancellationToken cancellationToken);
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
            Data = _mapper.Map<List<MeetingInvitationRecordDto>>(records)
        };
    }

    public async Task<GetMeetingInvitationUsersResponse> GetMeetingInvitationUsersAsync(GetMeetingInvitationUsersRequest request, CancellationToken cancellationToken)
    {
        var meetingUserSessions = await _meetingDataProvider.GetUserSessionsByMeetingIdAsync(meetingId: request.MeetingId, meetingSubId: request.MeetingSubId, includeUserName: true, onlineType: MeetingUserSessionOnlineType.Online, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        var staffs = await _smartiesClient.GetStaffDepartmentHierarchyTreeAsync(new GetStaffDepartmentHierarchyTreeRequest
        {
            StaffIdSource = request.StaffIdSource,
            HierarchyDepth = request.HierarchyDepth,
            HierarchyStaffRange = request.HierarchyStaffRange
        }, cancellationToken).ConfigureAwait(false);

        Log.Information("Invitation meeting user session: {@meetingUserSessions}", meetingUserSessions);
        
        UpdateStaffs(staffs.Data.StaffDepartmentHierarchy, meetingUserSessions.Select(x => x.UserName).ToList());
        
        return new GetMeetingInvitationUsersResponse
        {
            Data = staffs.Data
        };
    }

    public static void UpdateStaffs(List<GetStaffDepartmentHierarchyTreeRequestDto> staffDepartmentHierarchy, List<string> userNames)
    {
        foreach (var staff in staffDepartmentHierarchy)
        {
            if (staff.Staffs?.Any() == true)
            {
                foreach (var member in staff.Staffs)
                {
                    member.MeetingStaffStatus = userNames.Contains(member.UserName)
                        ? MeetingStaffStatus.Online
                        : MeetingStaffStatus.NoJoin;
                }
            }
            
            if (staff.Childrens?.Any() == true)
            {
                UpdateStaffs(staff.Childrens, userNames);
            }
        }
    }

    public async Task UpdateMeetingInvitationRecordAsync()
    {
    }

    public async Task<GetMeetingInvitationRecordsResponse> GetMeetingInvitationRecordsAsync(GetMeetingInvitationRecordsRequest request, CancellationToken cancellationToken)
    {
        if (_currentUser.Id == null)
            return null;

        return new GetMeetingInvitationRecordsResponse
        {
            Data = await _meetingDataProvider.GetMeetingInvitationRecordsDtoAsync(_currentUser.Id.Value, cancellationToken).ConfigureAwait(false)
        };
    }

    public async Task<UpdateMeetingInvitationRecordsResponse> UpdateMeetingInvitationRecordAsync(UpdateMeetingInvitationRecordsCommand command, CancellationToken cancellationToken)
    {
        var invitationRecordIds = command.MeetingInvitationRecords.Select(x => x.Id).ToList();

        if (invitationRecordIds.Count < 1)
            throw new Exception("Not meeting invitation record id");
        
        var meetingInvitationRecords = await _meetingDataProvider.GetMeetingInvitationRecordsAsync(invitationRecordIds, cancellationToken: cancellationToken).ConfigureAwait(false);

        foreach (var record in meetingInvitationRecords)
        {
            var status = command.MeetingInvitationRecords.FirstOrDefault(x => x.Id == record.Id);

            if (status == null) continue;
            
            record.InvitationStatus = status.InvitationStatus;
        }

        await _meetingDataProvider.UpdateMeetingInvitationRecordsAsync(meetingInvitationRecords, cancellationToken: cancellationToken).ConfigureAwait(false);

        return new UpdateMeetingInvitationRecordsResponse
        {
            Data = _mapper.Map<List<MeetingInvitationRecordDto>>(meetingInvitationRecords)
        };
    }
}