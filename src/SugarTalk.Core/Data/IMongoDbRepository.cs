using MongoDB.Driver.Linq;

namespace SugarTalk.Core.Data
{
    public interface IMongoDbRepository : IRepository
    {
        IMongoQueryable<T> Query<T>();
    }
}