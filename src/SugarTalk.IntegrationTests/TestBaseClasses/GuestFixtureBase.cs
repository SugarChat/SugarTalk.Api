using Xunit;

namespace SugarTalk.IntegrationTests.TestBaseClasses;

[Collection("Guest Tests")]
public class GuestFixtureBase : TestBase
{
    protected GuestFixtureBase() : base("_guest_", "sugar_talk_guest", 8)
    {
    }
}