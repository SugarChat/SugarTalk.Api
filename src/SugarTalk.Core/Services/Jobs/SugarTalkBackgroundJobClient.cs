using System;
using Hangfire;
using Hangfire.States;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SugarTalk.Core.Services.Jobs;

public interface ISugarTalkBackgroundJobClient : IScopedDependency
{
    string Enqueue<T>(Expression<Action> methodCall, string queue = "default");
    
    string Enqueue<T>(Expression<Action<T>> methodCall, string queue = "default");
    
    string Enqueue(Expression<Func<Task>> methodCall, string queue = "default");
    
    string Enqueue<T>(Expression<Func<T, Task>> methodCall, string queue = "default");

    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay, string queue = "default");
    
    string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt, string queue = "default");

    string ContinueJobWith(string parentJobId, Expression<Func<Task>> methodCall, string queue = "default");
        
    string ContinueJobWith<T>(string parentJobId, Expression<Func<T,Task>> methodCall, string queue = "default");
    
    void AddOrUpdateRecurringJob<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");
}

public class SugarTalkBackgroundJobClient : ISugarTalkBackgroundJobClient
{
    private readonly Func<IBackgroundJobClient> _backgroundJobClientFunc;
    private readonly Func<IRecurringJobManager> _recurringJobManagerFunc;
    
    public SugarTalkBackgroundJobClient(Func<IBackgroundJobClient> backgroundJobClientFunc, Func<IRecurringJobManager> recurringJobManagerFunc)
    {
        _backgroundJobClientFunc = backgroundJobClientFunc;
        _recurringJobManagerFunc = recurringJobManagerFunc;
    }

    public string Enqueue(Expression<Func<Task>> methodCall, string queue = "default")
    {
        return _backgroundJobClientFunc()?.Create(methodCall, new EnqueuedState(queue));
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall, string queue = "default")
    {
        return _backgroundJobClientFunc()?.Create(methodCall, new EnqueuedState(queue));
    }
    
    public string Enqueue<T>(Expression<Action> methodCall, string queue = "default")
    {
        return _backgroundJobClientFunc()?.Create(methodCall, new EnqueuedState(queue));
    }
    
    public string Enqueue<T>(Expression<Action<T>> methodCall, string queue = "default")
    {
        return _backgroundJobClientFunc()?.Create(methodCall, new EnqueuedState(queue));
    }
    
    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay, string queue = "default")
    {
        return _backgroundJobClientFunc()?.Schedule(queue, methodCall, delay);
    }
    
    public string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt, string queue = "default")
    {
        return _backgroundJobClientFunc()?.Schedule(queue, methodCall, enqueueAt);
    }
    
    public string ContinueJobWith(string parentJobId, Expression<Func<Task>> methodCall, string queue = "default")
    {
        return _backgroundJobClientFunc()?.ContinueJobWith(parentJobId, methodCall, new EnqueuedState(queue));
    }
    
    public string ContinueJobWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall, string queue = "default")
    {
        return _backgroundJobClientFunc()?.ContinueJobWith(parentJobId, methodCall, new EnqueuedState(queue));
    }

    public void AddOrUpdateRecurringJob<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
    {
        _recurringJobManagerFunc()?.AddOrUpdate(recurringJobId, queue, methodCall, cronExpression, new RecurringJobOptions
        {
            TimeZone = timeZone ?? TimeZoneInfo.Utc
        });
    }
}