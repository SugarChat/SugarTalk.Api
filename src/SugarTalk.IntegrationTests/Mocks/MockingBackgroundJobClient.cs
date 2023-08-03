using System;
using Autofac;
using System.Threading.Tasks;
using System.Linq.Expressions;
using SugarTalk.Core.Services.Jobs;

namespace SugarTalk.IntegrationTests.Mocks;

public class MockingBackgroundJobClient : ISugarTalkBackgroundJobClient
{
    private readonly IComponentContext _componentContext;

    public MockingBackgroundJobClient()
    {
    }
    
    public MockingBackgroundJobClient(IComponentContext componentContext)
    {
        _componentContext = componentContext;
    }

    public string Enqueue<T>(Expression<Action> methodCall, string queue = "default")
    {
        var func = methodCall.Compile();
        func();
        return string.Empty;
    }

    public string Enqueue<T>(Expression<Action<T>> methodCall, string queue = "default") where T : notnull
    {
        var dependency = _componentContext.Resolve<T>();
        var func = methodCall.Compile();
        func(dependency);
        return string.Empty;
    }

    public string Enqueue(Expression<Func<Task>> methodCall, string queue = "default")
    {
        var func = methodCall.Compile();
        func().Wait();
        return string.Empty;
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall, string queue = "default") where T : notnull
    {
        var dependency = _componentContext.Resolve<T>();
        var func = methodCall.Compile();
        func(dependency).Wait();
        return string.Empty;
    }

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay, string queue = "default")
    {
        var func = methodCall.Compile();
        func().Wait();
        return string.Empty;
    }

    public string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt, string queue = "default")
    {
        var func = methodCall.Compile();
        func().Wait();
        return string.Empty;
    }

    public string ContinueJobWith(string parentJobId, Expression<Func<Task>> methodCall, string queue = "default")
    {
        var func = methodCall.Compile();
        func().Wait();
        return string.Empty;
    }

    public string ContinueJobWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall, string queue = "default") where T : notnull
    {
        var dependency = _componentContext.Resolve<T>();
        var func = methodCall.Compile();
        func(dependency).Wait();
        return string.Empty;
    }

    public void AddOrUpdateRecurringJob<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
    {
        var dependency = _componentContext.Resolve<T>();
        var func = methodCall.Compile();
        func(dependency).Wait();
    }
}