using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Account;

public class LoginRequest : IRequest
{
    public string UserName { get; set; }
    
    public string Password { get; set; }
}

public class LoginResponse : SugarTalkResponse<string>
{
}