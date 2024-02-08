using SugarTalk.Core.Services.Identity;

namespace SugarTalk.IntegrationTests;

public class TestCurrentUser : InternalUser
{
    public string ThirdPartyId { get; set; } = "thirdpartyid";

    public string UserName { get; set; } = "TEST_USER";
}