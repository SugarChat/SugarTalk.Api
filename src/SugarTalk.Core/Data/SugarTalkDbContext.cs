using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain;
using SugarTalk.Core.Settings;
using SugarTalk.Core.Settings.System;

namespace SugarTalk.Core.Data;

public class SugarTalkDbContext : DbContext, IUnitOfWork
{
    private readonly SugarTalkConnectionString _connectionString;

    public SugarTalkDbContext(SugarTalkConnectionString connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(_connectionString.Value, new MySqlServerVersion(new Version(8, 0, 28)));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        typeof(SugarTalkDbContext).GetTypeInfo().Assembly.GetTypes()
            .Where(t => typeof(IEntity).IsAssignableFrom(t) && t.IsClass).ToList()
            .ForEach(x =>
            {
                if (modelBuilder.Model.FindEntityType(x) == null)
                    modelBuilder.Model.AddEntityType(x);
            });
    }
    
    public bool ShouldSaveChanges { get; set; }
}