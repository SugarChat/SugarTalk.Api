using Autofac;

namespace SugarTalk.IntegrationTests.Utils.Account;

public class AccountUtil : TestUtil
{
    protected AccountUtil(ILifetimeScope scope) : base(scope)
    {
    }
}