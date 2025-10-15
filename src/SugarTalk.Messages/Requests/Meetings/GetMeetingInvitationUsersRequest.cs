using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Smarties;
using SugarTalk.Messages.Enums.Smarties;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingInvitationUsersRequest : IRequest
{
    public Guid MeetingId { get; set; }
    
    public StaffIdSource StaffIdSource { get; set; } = StaffIdSource.Self;
    
    public HierarchyDepth HierarchyDepth { get; set; } = HierarchyDepth.Group;
    
    public HierarchyStaffRange HierarchyStaffRange { get; set; } = HierarchyStaffRange.All;
}

public class GetMeetingInvitationUsersResponse : SugarTalkResponse<GetStaffDepartmentHierarchyTreeResponseData>
{
}