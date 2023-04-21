using System.Linq.Expressions;
using Autofac;
using Mediator.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using SugarTalk.Api;
using SugarTalk.Core.Data;
using SugarTalk.E2ETests.Mocks;

namespace SugarTalk.E2ETests;

public class ApiTestFixture : WebApplicationFactory<Startup>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureContainer<ContainerBuilder>(b =>
        {
            RegisterDatabase(b);
            RegisterCurrentUser(b);
            RegisterHttpContextAccessor(b);
            // RegisterBackgroundJobClient(b);
        });
        return base.CreateHost(builder);
    }
    
    public override async ValueTask DisposeAsync()
    {
        await ClearDatabaseRecord();
        
        await base.DisposeAsync();
    }
    
    private async Task ClearDatabaseRecord()
    {
        await Services.GetRequiredService<InMemoryDbContext>().Database.EnsureDeletedAsync();
    }
    
    private void RegisterCurrentUser(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterInstance(new MockCurrentUser());
    }
    
    private void RegisterHttpContextAccessor(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterInstance(Substitute.For<IHttpContextAccessor>()).AsImplementedInterfaces();
    }
    
    private void RegisterDatabase(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<InMemoryDbContext>()
            .AsSelf()
            .As<DbContext>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    
        containerBuilder.RegisterType<InMemoryRepository>().As<IRepository>().InstancePerLifetimeScope();
    }
}