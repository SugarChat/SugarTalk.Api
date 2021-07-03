using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
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

        public async Task<List<T>> ToListAsync<T>(Expression<Func<T, bool>> predicate = null,
            CancellationToken cancellationToken = default) where T : class, IEntity
        {
            return await GetCollection<T>()
                .AsQueryable()
                .Where(predicate ?? (_ => true))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        private IMongoCollection<T> GetCollection<T>() => _database.GetCollection<T>(typeof(T).Name);
    }
}