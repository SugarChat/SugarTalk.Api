using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SugarTalk.Core.Domain;

namespace SugarTalk.Core.Data;

public interface IRepository
{
    ValueTask<TEntity> GetByIdAsync<TEntity>(object id, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    Task<List<TEntity>> GetAllAsync<TEntity>(CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    Task<List<TEntity>> ToListAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity;

    Task InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    Task InsertAllAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    Task UpdateAllAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    Task DeleteAllAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity;

    Task<TEntity?> SingleOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity;

    Task<TEntity?> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity;

    Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class, IEntity;

    Task<List<T>> SqlQueryAsync<T>(string sql, params object[] parameters) where T : class, IEntity;

    IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>>? predicate = null)
        where TEntity : class, IEntity;

    IQueryable<TEntity> QueryNoTracking<TEntity>(Expression<Func<TEntity, bool>>? predicate = null)
        where TEntity : class, IEntity;

    DatabaseFacade Database { get; }
    Task BatchInsertAsync<T>(IList<T> entities) where T : class, IEntity;
    Task BatchUpdateAsync<T>(IList<T> entities) where T : class, IEntity;
    Task BatchDeleteAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity;
}