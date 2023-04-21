using Xunit;

namespace SugarTalk.IntegrationTests.TestBaseClasses;

[Collection("Account Tests")]
public class AccountFixtureBase : TestBase
{
    protected AccountFixtureBase() : base("_account_", "sugartalk_account", 0)
    {
    }
}