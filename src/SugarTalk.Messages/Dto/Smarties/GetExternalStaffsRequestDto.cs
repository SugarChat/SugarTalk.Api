using System;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.Smarties;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Dto.Smarties;

public class GetExternalStaffsRequestDto
{
    public SyncCollectionFormSystemType? SystemType { get; set; } = SyncCollectionFormSystemType.WorkWechat;

    public List<Guid> Ids { get; set; }
}

public class GetExternalStaffsResponse : SugarTalkResponse<GetExternalStaffsResponseData>
{
}

public class GetExternalStaffsResponseData
{
    public int Count { get; set; }
    
    public List<ExternalStaffDto> Staffs { get; set; }
}

public class ExternalStaffDto
{
    public Guid StaffId { get; set; }
    
    public string ExternalSystemStaffId { get; set; }
    
    public SyncCollectionFormSystemType ExternalSystem { get; set; }
}
