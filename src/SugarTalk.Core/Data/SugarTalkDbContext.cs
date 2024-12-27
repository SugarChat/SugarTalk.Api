using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SugarTalk.Core.Data.Exceptions;
using SugarTalk.Core.Domain;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Core.Settings.System;

namespace SugarTalk.Core.Data;

public class SugarTalkDbContext : DbContext, IUnitOfWork
{
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly SugarTalkConnectionString _connectionString;
    public SugarTalkDbContext(SugarTalkConnectionString connectionString, ICurrentUser currentUser, IClock clock)
    {
        _clock = clock;
        _currentUser = currentUser;
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
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.DetectChanges();
        
        foreach (var entityEntry in ChangeTracker.Entries())
        {
            TrackCreated(entityEntry);
            TrackModification(entityEntry);
        }
         
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private void TrackCreated(EntityEntry entityEntry)
    {
        if (entityEntry.State == EntityState.Added && entityEntry.Entity is IHasCreatedFields createdEntity)
        {
            if (_currentUser is not { Id: not null } && createdEntity.CreatedBy == 0) throw new MissingCurrentUserWhenSavingNonNullableFieldException(nameof(createdEntity.CreatedBy));
            
            createdEntity.CreatedDate = _clock.Now;
            
            if (_currentUser?.Id != null && createdEntity.CreatedBy == 0 && _currentUser.Id.Value != CurrentUsers.InternalUser.Id)
                createdEntity.CreatedBy = _currentUser.Id.Value;
        }
    }
    
    private void TrackModification(EntityEntry entityEntry)
    {
        if (entityEntry.Entity is IHasModifiedFields modifyEntity && entityEntry.State is EntityState.Modified or EntityState.Added)
        { 
            if (_currentUser is not { Id: not null } && modifyEntity.LastModifiedBy == 0) throw new MissingCurrentUserWhenSavingNonNullableFieldException(nameof(modifyEntity.LastModifiedBy));
            
            modifyEntity.LastModifiedDate = _clock.Now;
            
            if (_currentUser?.Id != null && (modifyEntity.LastModifiedBy == 0 || _currentUser.Id.Value != CurrentUsers.InternalUser.Id))
                modifyEntity.LastModifiedBy = _currentUser.Id.Value;
        }
    }
    
    public bool ShouldSaveChanges { get; set; }
}