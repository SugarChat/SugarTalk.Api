using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using SugarTalk.Core.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace SugarTalk.Core.Data
{
    public class MongoDbRepository : IRepository
    {
        private readonly IMongoDatabase _database;

        public MongoDbRepository(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<T> SingleAsync<T>(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            return await GetCollection<T>()
                .AsQueryable()
                .Where(predicate ?? (_ => true))
                .SingleAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<T> SingleOrDefaultAsync<T>(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            return await GetCollection<T>()
                .AsQueryable()
                .Where(predicate ?? (_ => true))
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            return await GetCollection<T>()
                .AsQueryable()
                .Where(predicate ?? (_ => true))
                .FirstAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        
        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            return await GetCollection<T>()
                .AsQueryable()
                .Where(predicate ?? (_ => true))
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<int> CountAsync<T>(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            return await GetCollection<T>()
                .AsQueryable()
                .Where(predicate ?? (_ => true))
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> AnyAsync<T>(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            return await GetCollection<T>()
                .AsQueryable()
                .Where(predicate ?? (_ => true))
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<T>> ToListAsync<T>(Expression<Func<T, bool>> predicate = null,
            CancellationToken cancellationToken = default) where T : class, IEntity
        {
            return await GetCollection<T>()
                .AsQueryable()
                .Where(predicate ?? (_ => true))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            await GetCollection<T>().InsertOneAsync(entity, null, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddRangeAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            await GetCollection<T>().InsertManyAsync(entities, null, cancellationToken).ConfigureAwait(false);
        }

        public async Task RemoveAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);
            
            await GetCollection<T>().DeleteOneAsync(filter, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task RemoveRangeAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            var filter = Builders<T>.Filter.In(e => e.Id, entities.Select(e => e.Id));
            
            await GetCollection<T>().DeleteManyAsync(filter, null, cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);
            
            await GetCollection<T>().ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateRangeAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            var updates = entities.Select(entity =>
            {
                var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);

                return new ReplaceOneModel<T>(filter, entity);
            });
            
            await GetCollection<T>().BulkWriteAsync(updates, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private IMongoCollection<T> GetCollection<T>() => _database.GetCollection<T>(typeof(T).Name);
    }
}