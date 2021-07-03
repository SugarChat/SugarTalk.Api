using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Entities;

namespace SugarTalk.Core.Data
{
    public interface IRepository
    {
        Task<List<T>> ToListAsync<T>(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default) where T : class, IEntity;
    }
}