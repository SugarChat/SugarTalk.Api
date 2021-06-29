using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Entities;

namespace SugarTalk.Core
{
    public interface IDatabaseProvider
    {
        Task Insert<T>(T entity, CancellationToken cancellationToken) where T : IEntity;

        Task Update<T>(T entity, CancellationToken cancellationToken) where T : IEntity;

        Task Delete(Guid key, CancellationToken cancellationToken);
    }

    public class InMemoryDatabaseProvider : IDatabaseProvider
    {
        private Dictionary<Guid, object> _dictionary;
        public InMemoryDatabaseProvider()
        {
            _dictionary = new Dictionary<Guid, object>();
        }
        
        public Task Insert<T>(T entity, CancellationToken cancellationToken) where T : IEntity
        {
            _dictionary.Add(entity.Id, entity);
            return Task.CompletedTask;
        }

        public Task Update<T>(T entity, CancellationToken cancellationToken) where T : IEntity
        {
            _dictionary[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task Delete(Guid key, CancellationToken cancellationToken)
        {
            _dictionary.Remove(key);
            return Task.CompletedTask;
        }
    }
}