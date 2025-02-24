using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Smarties;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetStaffsTreeRequest : IRequest
{
}

public class GetStaffsTreeResponse : SugarTalkResponse<GetStaffDepartmentHierarchyTreeResponseData>
{
}