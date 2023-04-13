using System;
using System.Threading.Tasks;
using Autofac;
using SugarTalk.Core.Data;

namespace SugarTalk.Tests;

public class TestUtilBase
{
        private ILifetimeScope _scope;

    protected TestUtilBase(ILifetimeScope scope = null)
    {
        _scope = scope;
    }

    protected void SetupScope(ILifetimeScope scope) => _scope = scope;

    protected void Run<T>(Action<T> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var dependency = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration).Resolve<T>()
            : _scope.BeginLifetimeScope().Resolve<T>();
        action(dependency);
    }

    protected void Run<T, R>(Action<T, R> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var lifetime = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration)
            : _scope.BeginLifetimeScope();

        var dependency = lifetime.Resolve<T>();
        var dependency2 = lifetime.Resolve<R>();
        action(dependency, dependency2);
    }

    protected void Run<T, R, L>(Action<T, R, L> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var lifetime = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration)
            : _scope.BeginLifetimeScope();

        var dependency = lifetime.Resolve<T>();
        var dependency2 = lifetime.Resolve<R>();
        var dependency3 = lifetime.Resolve<L>();
        action(dependency, dependency2, dependency3);
    }

    protected async Task Run<T, R, L>(Func<T, R, L, Task> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var lifetime = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration)
            : _scope.BeginLifetimeScope();

        var dependency = lifetime.Resolve<T>();
        var dependency2 = lifetime.Resolve<R>();
        var dependency3 = lifetime.Resolve<L>();
        await action(dependency, dependency2, dependency3);
    }

    protected async Task Run<T>(Func<T, Task> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var dependency = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration).Resolve<T>()
            : _scope.BeginLifetimeScope().Resolve<T>();
        await action(dependency);
    }
    
    protected async Task RunWithUnitOfWork<T>(Func<T, Task> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var scope = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration)
            : _scope.BeginLifetimeScope();

        var dependency = scope.Resolve<T>();
        var unitOfWork = scope.Resolve<IUnitOfWork>();

        await action(dependency);
        await unitOfWork.SaveChangesAsync();
    }

    protected async Task<R> Run<T, R>(Func<T, Task<R>> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var dependency = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration).Resolve<T>()
            : _scope.BeginLifetimeScope().Resolve<T>();
        return await action(dependency);
    }
    
    protected async Task<R> RunWithUnitOfWork<T, R>(Func<T, Task<R>> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var scope = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration)
            : _scope.BeginLifetimeScope();

        var dependency = scope.Resolve<T>();
        var unitOfWork = scope.Resolve<IUnitOfWork>();

        var result = await action(dependency);
        await unitOfWork.SaveChangesAsync();

        return result;
    }

    protected async Task<R> RunWithUnitOfWork<T, U, R>(Func<T, U, Task<R>> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var scope = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration)
            : _scope.BeginLifetimeScope();

        var dependency = scope.Resolve<T>();
        var dependency1 = scope.Resolve<U>();
        var unitOfWork = scope.Resolve<IUnitOfWork>();

        var result = await action(dependency, dependency1);
        await unitOfWork.SaveChangesAsync();

        return result;
    }
    
    protected async Task RunWithUnitOfWork<T, U>(Func<T, U, Task> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var scope = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration)
            : _scope.BeginLifetimeScope();

        var dependency = scope.Resolve<T>();
        var dependency2 = scope.Resolve<U>();
        var unitOfWork = scope.Resolve<IUnitOfWork>();

        await action(dependency, dependency2);
        await unitOfWork.SaveChangesAsync();
    }

    protected R Run<T, R>(Func<T, R> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var dependency = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration).Resolve<T>()
            : _scope.BeginLifetimeScope().Resolve<T>();
        return action(dependency);
    }

    protected R Run<T, U, R>(Func<T, U, R> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var lifetime = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration)
            : _scope.BeginLifetimeScope();

        var dependency = lifetime.Resolve<T>();
        var dependency2 = lifetime.Resolve<U>();
        return action(dependency, dependency2);
    }

    protected Task Run<T, U>(Func<T, U, Task> action, Action<ContainerBuilder> extraRegistration = null)
    {
        var lifetime = extraRegistration != null
            ? _scope.BeginLifetimeScope(extraRegistration)
            : _scope.BeginLifetimeScope();
        var dependency = lifetime.Resolve<T>();
        var dependency2 = lifetime.Resolve<U>();
        return action(dependency, dependency2);
    }
}