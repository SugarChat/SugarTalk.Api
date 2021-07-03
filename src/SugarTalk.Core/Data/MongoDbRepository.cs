using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using SugarTalk.Core.Entities;
using MongoDB.Driver;

namespace SugarTalk.Core.Data
{
    public class MongoDbRepository : IRepository
    {
        private readonly IMongoDatabase _database;

        public MongoDbRepository(IMongoDatabase database)
        {
            _database = database;
        }
        
        public Task<List<T>> ToListAsync<T>(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            return Task.FromResult(new List<T>());
        }
    }
}