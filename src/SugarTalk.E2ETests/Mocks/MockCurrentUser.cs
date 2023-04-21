using SugarTalk.Messages.Enums;

namespace SugarTalk.E2ETests.Mocks;

public class MockCurrentUser
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string ThirdPartyId { get; set; } = "TestThirdPartyId";

    public string Email { get; set; } = "test@email.com";

    public string Picture { get; set; } = "https://www.sugartalk.com/test-picture.png";

    public ThirdPartyFrom AuthType { get; } = ThirdPartyFrom.Google;

    public string DisplayName { get; set; } = "TestName";
}