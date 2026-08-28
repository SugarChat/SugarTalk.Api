using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Constants;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Account;

[SugarTalkAuthorize(SecurityStore.Roles.Administrator)]
public class AddApiKeyPermissionCommand : ICommand
{
    public string ApiKey { get; set; }

    public string PermissionName { get; set; }
}

public class AddApiKeyPermissionResponse : SugarTalkResponse<bool>
{
}
