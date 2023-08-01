using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.States;
using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.Jobs;

public interface ISugarTalkBackgroundJobClient : IScopedDependency
{
    string Enqueue(Expression<Func<Task>> methodCall);
    
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);
    
    string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt);
    
    string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);
    
    string ContinueJobWith(string parentJobId, Expression<Func<Task>> methodCall);
        
    string ContinueJobWith<T>(string parentJobId, Expression<Func<T,Task>> methodCall);

    bool DeleteJob(string jobId);

    void RemoveRecurringJobIfExists(string jobId);

    void AddOrUpdateRecurringJob<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression,
        TimeZoneInfo timeZone = null, string queue = "default");
}

public class SugarTalkBackgroundJobClient : ISugarTalkBackgroundJobClient
{
    private readonly EnqueuedState _queue;  
    private readonly Func<IRecurringJobManager> _recurringJobManagerFunc;
    private readonly Func<IBackgroundJobClient> _backgroundJobClientFunc;

    public SugarTalkBackgroundJobClient(
        Func<IRecurringJobManager> recurringJobManagerFunc,
        Func<IBackgroundJobClient> backgroundJobClientFunc)
    {
        _recurringJobManagerFunc = recurringJobManagerFunc;
        _backgroundJobClientFunc = backgroundJobClientFunc;

        _queue = new EnqueuedState("default");
    }
    
    public string Enqueue(Expression<Func<Task>> methodCall)
    {
        return _backgroundJobClientFunc()?.Create(methodCall, _queue);
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        return _backgroundJobClientFunc()?.Create(methodCall, _queue);
    }

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        return _backgroundJobClientFunc()?.Schedule(methodCall, delay);
    }

    public string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
    {
        return _backgroundJobClientFunc()?.Schedule(methodCall, enqueueAt);
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
    {
        return _backgroundJobClientFunc()?.Schedule(methodCall, enqueueAt);
    }

    public string ContinueJobWith(string parentJobId, Expression<Func<Task>> methodCall)
    {
        return _backgroundJobClientFunc()?.ContinueJobWith(parentJobId, methodCall, _queue);
    }

    public string ContinueJobWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall)
    {
        return _backgroundJobClientFunc()?.ContinueJobWith(parentJobId, methodCall, _queue);
    }

    public bool DeleteJob(string jobId)
    {
        return _backgroundJobClientFunc().Delete(jobId);
    }

    public void RemoveRecurringJobIfExists(string jobId)
    {
        _recurringJobManagerFunc()?.RemoveIfExists(jobId);
    }

    public void AddOrUpdateRecurringJob<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression,
        TimeZoneInfo timeZone = null, string queue = "default")
    {
        _recurringJobManagerFunc().AddOrUpdate(recurringJobId, methodCall, cronExpression, timeZone, queue);
    }
}