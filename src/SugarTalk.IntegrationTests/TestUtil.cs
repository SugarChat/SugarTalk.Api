using Autofac;

namespace SugarTalk.Tests;

public class TestUtil : TestUtilBase
{
    protected TestUtil(ILifetimeScope scope)
    {
        SetupScope(scope);
    }
}