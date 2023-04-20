using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Account;

public class RegisterCommand : ICommand
{
    public string UserName { get; set; }
    
    public string Password { get; set; }
}

public class RegisterResponse : SugarTalkResponse<bool>
{
}