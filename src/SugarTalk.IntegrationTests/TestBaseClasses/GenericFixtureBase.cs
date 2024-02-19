using Xunit;

namespace SugarTalk.IntegrationTests.TestBaseClasses;

[Collection("Generic Tests")]
public class GenericFixtureBase : TestBase
{
    protected GenericFixtureBase() : base("_generic_", "sugar_talk_generic", 8)
    {
    }
}