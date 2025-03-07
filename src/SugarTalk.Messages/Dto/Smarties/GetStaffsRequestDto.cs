using System;
using System.Collections.Generic;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Dto.Smarties;

public class GetStaffsRequestDto
{
    public bool IsActive { get; set; } = true;

    public List<Guid> Ids { get; set; } = null;

    public List<Guid> UserIds { get; set; } = null;
}

public class GetStaffsResponse : SugarTalkResponse<GetStaffsResponseData>
{
}

public class GetStaffsResponseData
{
    public List<RmStaffDto> Staffs { get; set; }
}

public class RmStaffDto
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }
    
    public string UserName { get; set; }
}