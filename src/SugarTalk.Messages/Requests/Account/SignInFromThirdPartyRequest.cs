using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Account;

public class SignInFromThirdPartyRequest : IRequest
{
}

public class SignInFromThirdPartyResponse : SugarTalkResponse<SignedInUserDto>
{
}