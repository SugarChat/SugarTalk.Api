using SugarTalk.Core.Services.Identity;
using SugarTalk.Messages.Enums.Account;

namespace SugarTalk.IntegrationTests;

public class TestCurrentUser : ICurrentUser
{
    public int? Id { get; init; } = 1;
    
    public string Name { get; } = "TEST_USER";

    public UserAccountIssuer AuthType { get; set; }

    public string ThirdPartyId { get; set; } = "thirdpartyid";

    public string UserName { get; set; } = "TEST_USER";
}