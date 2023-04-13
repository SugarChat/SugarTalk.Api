using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SugarTalk.Core.Domain;

namespace SugarTalk.Core.Data;

public class EfRepository : IRepository
{
    private readonly SugarTalkDbContext _dbContext;

    public EfRepository(SugarTalkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ValueTask<TEntity> GetByIdAsync<TEntity>(object id,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        return _dbContext.FindAsync<TEntity>(new object[] { id }, cancellationToken);
    }

    public Task<List<TEntity>> GetAllAsync<TEntity>(CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        return _dbContext.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public Task<List<TEntity>> ToListAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        return _dbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task InsertAsync<TEntity>(TEntity entity,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        await _dbContext.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        _dbContext.ShouldSaveChanges = true;
    }

    public async Task InsertAllAsync<TEntity>(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        await _dbContext.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        _dbContext.ShouldSaveChanges = true;
    }

    public Task UpdateAsync<TEntity>(TEntity entity,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        _dbContext.Update(entity);
        _dbContext.ShouldSaveChanges = true;
        return Task.CompletedTask;
    }

    public Task UpdateAllAsync<TEntity>(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        _dbContext.UpdateRange(entities);
        _dbContext.ShouldSaveChanges = true;
        return Task.CompletedTask;
    }

    public Task DeleteAsync<TEntity>(TEntity entity,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        _dbContext.Remove(entity);
        _dbContext.ShouldSaveChanges = true;
        return Task.CompletedTask;
    }

    public Task DeleteAllAsync<TEntity>(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        _dbContext.RemoveRange(entities);
        _dbContext.ShouldSaveChanges = true;
        return Task.CompletedTask;
    }

    public Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        return _dbContext.Set<TEntity>().CountAsync(predicate, cancellationToken);
    }

    public Task<TEntity?> SingleOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        return _dbContext.Set<TEntity>().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<TEntity?> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        return _dbContext.Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {
        return _dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    public async Task<List<TEntity>> SqlQueryAsync<TEntity>(string sql, params object[] parameters) where TEntity : class, IEntity
    {
        return await _dbContext.Set<TEntity>().FromSqlRaw(sql, parameters).ToListAsync();
    }

    public IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>>? predicate = null)
        where TEntity : class, IEntity
    {
        return predicate == null ? _dbContext.Set<TEntity>() : _dbContext.Set<TEntity>().Where(predicate);
    }

    public IQueryable<TEntity> QueryNoTracking<TEntity>(Expression<Func<TEntity, bool>>? predicate = null)
        where TEntity : class, IEntity
    {
        return predicate == null
            ? _dbContext.Set<TEntity>().AsNoTracking()
            : _dbContext.Set<TEntity>().AsNoTracking().Where(predicate);
    }

    public DatabaseFacade Database => _dbContext.Database;

    public async Task BatchInsertAsync<TEntity>(IList<TEntity> entities) where TEntity : class, IEntity
    {
        await _dbContext.BulkInsertAsync<TEntity>(entities).ConfigureAwait(false);
    }

    public async Task BatchUpdateAsync<TEntity>(IList<TEntity> entities) where TEntity : class, IEntity
    {
        await _dbContext.BulkUpdateAsync<TEntity>(entities).ConfigureAwait(false);
    }

    public async Task BatchDeleteAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity
    {
        await _dbContext.Set<T>().Where(predicate).BatchDeleteAsync().ConfigureAwait(false);
    }
}