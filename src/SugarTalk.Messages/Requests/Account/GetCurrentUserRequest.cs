using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Account;

public class GetCurrentUserRequest : IRequest
{
}

public class GetCurrentUserResponse : SugarTalkResponse<UserAccountDto>
{
}