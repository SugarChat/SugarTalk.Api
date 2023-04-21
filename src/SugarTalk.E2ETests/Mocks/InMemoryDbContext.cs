using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain;

namespace SugarTalk.E2ETests.Mocks;

public class InMemoryDbContext : DbContext, IUnitOfWork
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(databaseName: "SugarTalk");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        typeof(SugarTalkModule).GetTypeInfo().Assembly.GetTypes()
            .Where(t => typeof(IEntity).IsAssignableFrom(t) && t.IsClass).ToList()
            .ForEach(x =>
            {
                if (modelBuilder.Model.FindEntityType(x) == null)
                    modelBuilder.Model.AddEntityType(x);
            });
    }
    
    public bool ShouldSaveChanges { get; set; }
}