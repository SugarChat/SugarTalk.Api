using System;
using Autofac;
using Hangfire.Storage;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using SugarTalk.Core.Services.Jobs;

namespace SugarTalk.IntegrationTests.Mocks;

public class MockingBackgroundJobClient : ISugarTalkBackgroundJobClient
{
    public static readonly List<string> TestJobs = new ();

    public static readonly List<string> TestStore = new ();
    
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
        return nameof(Enqueue);
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
    
    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay, string queue = "default")
    {
        var dependency = _componentContext.Resolve<T>();
        var func = methodCall.Compile();
        func(dependency).Wait();
        TestJobs.Add("Add");
        return nameof(Schedule);
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt, string queue = "default")
    {
        TestJobs.Add("add Delayed");
        return "";
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
        return nameof(ContinueJobWith);
    }

    public void AddOrUpdateRecurringJob<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
    {
        var dependency = _componentContext.Resolve<T>();
        var func = methodCall.Compile();
        TestJobs.Add("Add Recurring");
        func(dependency).Wait();
    }

    public bool DeleteJob(string jobId)
    {
        return TestJobs.Count > 0 ? TestJobs.Remove(jobId) : default;
    }

    public void RemoveRecurringJobIfExists(string jobId)
    {
        TestStore.Add("Trigger remove");
    }

    public List<RecurringJobDto> GetRecurringJobs()
    {
        return new List<RecurringJobDto>();
    }

    public StateData GetJobState(string jobId)
    {
        return new StateData();
    }
}