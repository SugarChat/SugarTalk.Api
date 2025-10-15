using System;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.Smarties;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Dto.Smarties;

public class GetStaffDepartmentHierarchyTreeRequest
{
    public StaffIdSource StaffIdSource { get; set; } = StaffIdSource.UserAccount;
    
    public HierarchyDepth HierarchyDepth { get; set; } = HierarchyDepth.Group;
    
    public HierarchyStaffRange HierarchyStaffRange { get; set; } = HierarchyStaffRange.All;
}

public class GetStaffDepartmentHierarchyTreeResponse : SugarTalkResponse<GetStaffDepartmentHierarchyTreeResponseData>
{
}

public class GetStaffDepartmentHierarchyTreeResponseData
{
    public List<GetStaffDepartmentHierarchyTreeRequestDto> StaffDepartmentHierarchy { get; set; }
}

public class GetStaffDepartmentHierarchyTreeRequestDto
{
    public RmUnitHierarchyDto Department { get; set; }
    
    public List<StaffHierarchyUserDto> Staffs { get; set; }

    public List<GetStaffDepartmentHierarchyTreeRequestDto> Childrens { get; set; } = new();
}

public class RmUnitHierarchyDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public Guid ParentId { get; set; }
}

public class StaffHierarchyUserDto
{
    public string Id { get; set; }
    
    public string UserName { get; set; }

    public MeetingStaffStatus MeetingStaffStatus { get; set; }
}