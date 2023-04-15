using Autofac;

namespace SugarTalk.IntegrationTests;

public class TestUtil : TestUtilBase
{
    protected TestUtil(ILifetimeScope scope)
    {
        SetupScope(scope);
    }
}